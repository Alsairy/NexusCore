# Saudi Kit Operations Guide

## ZATCA E-Invoicing

### Onboarding Workflow

1. **Register with ZATCA** — Obtain EGS (E-Invoice Generation Solution) credentials from ZATCA's Fatoora portal
2. **Configure Settings** — Navigate to Saudi Settings > ZATCA tab
   - Set Environment (Sandbox → Simulation → Production)
   - Enter API Base URL
   - Enter Compliance CSID and Secret
3. **Create a Seller** — Navigate to ZATCA > Sellers
   - Enter Arabic name, VAT number (15 digits), and address
   - Set as default seller
4. **Upload Certificate** — Navigate to Settings > Certificates
   - Select the seller
   - Upload the certificate PEM and private key from ZATCA
   - Set environment and activate
5. **Create & Submit Invoices**
   - Draft → Validate → Submit to ZATCA
   - Simplified invoices are "reported" (async)
   - Standard invoices are "cleared" (sync)

### Invoice Types

| Type | Description | ZATCA Flow |
|------|-------------|------------|
| Standard (B2B) | Business-to-business | Clearance (synchronous) |
| Simplified (B2C) | Business-to-consumer | Reporting (asynchronous) |
| Credit Note | Reversal of original invoice | Same as original type |
| Debit Note | Adjustment to original invoice | Same as original type |

### Invoice Statuses

| Status | Description |
|--------|-------------|
| Draft | Created, not yet submitted |
| Validated | Passed local validation |
| Reported | Submitted to ZATCA (simplified) |
| Cleared | Cleared by ZATCA (standard) |
| Rejected | Failed ZATCA validation |
| Archived | Archived for record-keeping |

### Troubleshooting

| Issue | Resolution |
|-------|------------|
| "ZATCA API settings are not configured" | Go to Settings > ZATCA and configure API URL, CSID, and Secret |
| Invoice rejected with validation errors | Check ZATCA error messages in invoice detail; common issues: missing buyer VAT (B2B), invalid date format |
| QR code not generating | Ensure the invoice has been submitted and ZATCA returned a QR code |
| Circuit breaker open | ZATCA API may be down; wait 30 seconds for automatic recovery |

## Nafath SSO

### Setup with Elm

1. **Register with Elm** — Obtain AppId and AppKey from Elm's developer portal
2. **Configure Settings** — Navigate to Saudi Settings > Nafath tab
   - Enter AppId and AppKey
   - Set API Base URL
   - Configure Callback URL (must be publicly accessible)
3. **Set Timeout** — Default is 60 seconds; adjust based on expected user response time

### Authentication Flow

1. User enters 10-digit National ID
2. System calls Nafath API → receives transaction ID + random number
3. User opens Nafath app on phone → selects the displayed number
4. System polls status every 3 seconds (max 20 polls = 60 seconds)
5. On completion → user identity is verified

### Identity Linking

- Users can link their Nafath identity to their NexusCore account
- Once linked, the National ID is stored and verified
- Users can unlink their identity at any time
- Only one Nafath identity per user account

### Troubleshooting

| Issue | Resolution |
|-------|------------|
| "Nafath settings are not properly configured" | Configure AppId, AppKey, and API URL in Settings |
| Authentication timeout | User has 60 seconds to approve in Nafath app; retry if expired |
| "Failed to communicate with Nafath API" | Check network connectivity and Nafath API status |

## Approval Workflows

### How It Works

- Approval tasks are created when business actions require authorization
- Tasks are assigned to specific users or roles
- Assignees can approve or reject with optional comments
- Delegation allows users to transfer approval authority during absence

### Delegation

- Set delegate user, start date, and end date
- Active delegations automatically route tasks to the delegate
- Multiple delegations can be active simultaneously
- Delegations expire automatically after end date

### Hijri Calendar

- Provides server-side Hijri date conversion
- Falls back to client-side `Intl.DateTimeFormat` if API is unavailable
- Dual calendar picker supports both Gregorian and Hijri date input
- Invoice dates include both Gregorian and Hijri representations
