# ADR-003: Nafath Polling vs Webhook Strategy

## Status

Accepted

## Date

2024-12-20

## Context

Nafath is Saudi Arabia's national digital identity service operated by Elm. The authentication flow is:

1. User enters their 10-digit National ID in our app
2. Our backend calls Nafath API → receives a `transactionId` + `random` number
3. User opens the Nafath mobile app and selects the matching random number
4. Our app needs to know when the user has completed (or rejected/expired) the authentication

The question is: **how does our app learn about the authentication result?**

Nafath's API supports two patterns:

1. **Polling** — Our backend periodically calls `GET /status/{transactionId}` to check if the user has responded. Simple to implement, no infrastructure requirements. Downside: introduces latency (polling interval), wastes API calls, and risks hitting Nafath rate limits.

2. **Webhook/Callback** — Nafath calls a URL we provide when the user responds. Instant notification, no wasted API calls. Downside: requires a publicly accessible endpoint, signature validation for security, and handling for missed/delayed callbacks.

Additional constraints:
- Nafath authentication has a **60-second timeout** — the user must respond within this window
- Our platform is **multi-tenant** — multiple tenants may authenticate simultaneously
- The Angular frontend needs real-time feedback (showing "Waiting..." → "Approved")

## Decision

We chose a **hybrid approach: polling as primary, callback as accelerator.**

### Primary: Client-driven polling

```
Angular                    Backend                      Nafath API
  │                          │                              │
  ├─ POST /nafath/login ────►│                              │
  │                          ├─ POST /request ─────────────►│
  │                          │◄── { transactionId, random } ─┤
  │◄── { transactionId,     ─┤                              │
  │      random }            │                              │
  │                          │                              │
  ├─ GET /nafath/status ────►│                              │
  │    (every 3 seconds)     ├─ GET /status/{txnId} ───────►│
  │                          │◄── { status: "WAITING" } ────┤
  │◄── { status: "WAITING" } ┤                              │
  │                          │                              │
  │  ... (repeat polling) ...│                              │
  │                          │                              │
  ├─ GET /nafath/status ────►│                              │
  │                          ├─ GET /status/{txnId} ───────►│
  │                          │◄── { status: "COMPLETED" } ──┤
  │◄── { status: "COMPLETED"}┤                              │
  │                          │                              │
  │  ✓ Authentication done   │                              │
```

- Angular polls `GET /api/app/nafath/check-status/{transactionId}` every 3 seconds
- Backend forwards to Nafath API, caches the response briefly
- Maximum 20 polls (60 seconds ÷ 3 seconds)
- Frontend shows spinner with the random number during polling

### Secondary: Callback endpoint as accelerator

```
Nafath API ──► POST /api/saudi/nafath/callback
               │
               ├─ Validate request (rate limiting)
               ├─ Update NafathAuthRequest entity status
               └─ Next poll from Angular picks up the updated status immediately
```

- `NafathCallbackController` receives async status updates from Nafath
- `[AllowAnonymous]` with rate limiting (10 req/60s)
- Updates the `NafathAuthRequest` entity in the database
- The next Angular poll reads the already-updated status → faster response

The callback doesn't replace polling — it accelerates it. If the callback arrives between polls, the next poll returns the result immediately instead of waiting for the Nafath API round-trip.

### Why not callback-only?

1. **Public URL requirement** — In development and sandbox environments, developers often don't have a publicly accessible URL. Polling works everywhere.
2. **Callback reliability** — If Nafath's callback delivery fails or is delayed, polling ensures the user still gets a result.
3. **Simpler frontend** — Polling with `setInterval` is simpler than WebSocket/SSE for a 60-second window. No connection management, no reconnection logic.
4. **Multi-tenant routing** — Polling naturally scopes to the current user's transaction. Callbacks require routing the response to the correct tenant and user.

### Why not polling-only?

1. **Latency** — Without the callback, users wait up to 3 seconds after approving in Nafath. With the callback updating the DB, the next poll is instant.
2. **Future WebSocket upgrade** — If we later add WebSocket support, the callback can push directly to the frontend, eliminating polling entirely. The callback infrastructure is already in place.

## Consequences

**Positive:**
- Works in all environments (local dev, sandbox, production) — polling needs no public URL
- Resilient to callback delivery failures — polling is the fallback
- Faster user experience when callback works — reduces perceived latency
- Simple Angular implementation (RxJS `interval` + `takeWhile`)
- Callback infrastructure ready for future WebSocket/SSE upgrade

**Negative:**
- Polling generates more Nafath API calls than pure callback (up to 20 per auth attempt)
- 3-second polling interval means up to 3 seconds of unnecessary wait time
- Two code paths to maintain (polling + callback) instead of one
- Callback endpoint is `[AllowAnonymous]` — requires rate limiting and input validation to prevent abuse
- No HMAC signature validation on Nafath callbacks (Nafath doesn't provide one) — relies on rate limiting and input validation for security
