# Saudi Kit Security Review

## AllowAnonymous Endpoints

| Endpoint | Purpose | Protections |
|----------|---------|-------------|
| `POST /api/saudi/nafath/callback` | Nafath SSO callback | Rate limiting (10 req/60s), input validation |
| `GET /api/saudi/nafath/callback/health` | Nafath health check | Read-only, no sensitive data |
| `POST /api/saudi/zatca/webhook` | ZATCA async response | HMAC signature validation, rate limiting (10 req/60s) |
| `GET /api/saudi/zatca/webhook/health` | ZATCA health check | Read-only, no sensitive data |

## Input Validation

### ZATCA Invoice
- Invoice number: required, string
- Seller ID: required, valid GUID
- Issue date: required, valid date
- Buyer VAT number: validated format when present (15-digit numeric)
- Line items: at least one required, quantities/prices must be positive
- Currency code: validated against ISO 4217

### Nafath
- National ID: exactly 10 digits, numeric only
- Transaction ID: required for status checks
- Callback request: TransactionId and Status required

### Approval Workflows
- Task ID: required, valid GUID
- Comment: optional, max length enforced
- Delegation dates: end date must be after start date

## Encrypted Settings

Sensitive settings are stored using ABP's `ISettingManager` with `SetForCurrentTenantAsync`:

| Setting | Type |
|---------|------|
| `Saudi.Zatca.ComplianceCsid` | Encrypted at rest |
| `Saudi.Zatca.ProductionCsid` | Encrypted at rest |
| `Saudi.Zatca.Secret` | Encrypted at rest |
| `Saudi.Nafath.AppKey` | Encrypted at rest |
| `Saudi.Zatca.WebhookSecret` | Configuration only |

ABP uses `StringEncryption.DefaultPassPhrase` for encryption. Ensure this is set to a strong, unique value in production.

## Rate Limiting

| Policy | Limit | Window | Queue |
|--------|-------|--------|-------|
| `NafathCallback` | 10 requests | 60 seconds | 2 queued |
| `ZatcaWebhook` | 10 requests | 60 seconds | 2 queued |

Excess requests receive HTTP 429 (Too Many Requests).

## HMAC Webhook Validation

ZATCA webhook requests are validated using HMAC-SHA256:
- Header: `X-ZATCA-Signature`
- Secret: configured in `Saudi:Zatca:WebhookSecret`
- Algorithm: HMAC-SHA256 of raw request body
- If no secret is configured (dev/sandbox), validation is skipped

## Resilience Policies

### ZATCA API Client
- **Retry**: 3 attempts, exponential backoff (1s, 2s, 4s)
- **Circuit Breaker**: Opens after 50% failure rate (min 5 requests in 30s), breaks for 30s
- **Timeout**: 30 seconds per request
- Only retries on 5xx server errors and `HttpRequestException`

### Nafath API Client
- **Retry**: 2 attempts, exponential backoff (2s, 4s)
- **Circuit Breaker**: Same as ZATCA (50% failure, 5 min throughput, 30s break)
- **Timeout**: 15 seconds per request

## Authorization

All Saudi Kit app service endpoints require authentication except callbacks/webhooks. Permissions:

| Permission | Description |
|------------|-------------|
| `NexusCore.Zatca.Invoices` | View invoices |
| `NexusCore.Zatca.InvoicesCreate` | Create invoices |
| `NexusCore.Zatca.InvoicesEdit` | Edit draft invoices |
| `NexusCore.Zatca.InvoicesDelete` | Delete draft invoices |
| `NexusCore.Zatca.InvoicesSubmit` | Submit to ZATCA |
| `NexusCore.Zatca.Sellers` | Manage sellers |
| `NexusCore.Zatca.CertificatesManage` | Manage certificates |
| `NexusCore.Zatca.ManageSettings` | Configure ZATCA/Nafath settings |

## Recommendations

1. **Rotate secrets regularly** — ZATCA CSID, Nafath AppKey, webhook secrets
2. **Monitor circuit breaker** — Track `saudikit.zatca.api.duration` histogram for degradation
3. **Enable HTTPS** — All production traffic must use TLS 1.2+
4. **Audit logging** — ABP audit logging is enabled for all authenticated endpoints
5. **Network segmentation** — Restrict callback/webhook endpoints to known ZATCA/Nafath IP ranges if possible
