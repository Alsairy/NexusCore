# NexusCore Deployment Guide

## Prerequisites

- .NET 10 SDK
- Node.js 22+
- SQL Server 2022 (or Azure SQL)
- Docker & Docker Compose (for containerized deployment)

## Configuration

### appsettings.Production.json

```json
{
  "App": {
    "SelfUrl": "https://api.yourdomain.com",
    "AngularUrl": "https://app.yourdomain.com",
    "CorsOrigins": "https://app.yourdomain.com",
    "RedirectAllowedUrls": "https://app.yourdomain.com",
    "DisablePII": true,
    "HealthCheckUrl": "/health-status"
  },
  "ConnectionStrings": {
    "Default": "Server=your-db-server;Database=NexusCore;User Id=nexuscore_app;Password=<STRONG_PASSWORD>;TrustServerCertificate=false;Encrypt=true"
  },
  "AuthServer": {
    "Authority": "https://api.yourdomain.com",
    "RequireHttpsMetadata": true,
    "SwaggerClientId": "NexusCore_Swagger",
    "CertificatePassPhrase": "<GENERATE_NEW_GUID>"
  },
  "StringEncryption": {
    "DefaultPassPhrase": "<GENERATE_16_CHAR_KEY>"
  },
  "Saudi": {
    "Zatca": {
      "Environment": "Production",
      "ApiBaseUrl": "https://gw-fatoora.zatca.gov.sa",
      "WebhookSecret": "<HMAC_SECRET_FROM_ZATCA>"
    },
    "Nafath": {
      "ApiBaseUrl": "https://nafath.api.elm.sa",
      "CallbackUrl": "https://api.yourdomain.com/api/saudi/nafath/callback"
    }
  }
}
```

### Environment Variables (Docker)

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__Default` | SQL Server connection string |
| `App__SelfUrl` | API base URL |
| `App__AngularUrl` | Angular app URL |
| `App__CorsOrigins` | Allowed CORS origins |
| `AuthServer__Authority` | OpenIddict authority URL |
| `AuthServer__RequireHttpsMetadata` | Set `true` in production |

## Docker Compose Deployment

```bash
# Build and start all services
docker compose up -d

# Check health
curl http://localhost:44333/health-status

# View logs
docker compose logs -f api
```

### Services

| Service | Port | Description |
|---------|------|-------------|
| `db` | 1433 | SQL Server 2022 |
| `api` | 44333 (mapped to 8080) | .NET API + OpenIddict |
| `angular` | 4200 (mapped to 80) | Angular SPA via nginx |

## Manual Deployment

### Backend

```bash
dotnet publish src/NexusCore.HttpApi.Host/NexusCore.HttpApi.Host.csproj \
  -c Release -o publish/api

# Run database migrations
dotnet NexusCore.DbMigrator.dll

# Start the API
cd publish/api
dotnet NexusCore.HttpApi.Host.dll
```

### Frontend

```bash
cd angular
npm ci --legacy-peer-deps
npx ng build --configuration production

# Deploy dist/NexusCore/browser/ to nginx or CDN
```

## Health Checks

| Endpoint | Description |
|----------|-------------|
| `/health-status` | All health checks (DB, ZATCA, Nafath) |
| `/health-ui` | Health check dashboard UI |
| `/api/saudi/zatca/webhook/health` | ZATCA webhook endpoint health |
| `/api/saudi/nafath/callback/health` | Nafath callback endpoint health |

## SSL/TLS

- Generate an OpenIddict certificate: `dotnet dev-certs https -ep openiddict.pfx -p <passphrase>`
- For production, use a proper X.509 certificate from a trusted CA
- Configure the certificate passphrase in `AuthServer:CertificatePassPhrase`

## Database

- SQL Server 2022 or later recommended
- Run `NexusCore.DbMigrator` for initial setup and migrations
- The migrator seeds required data (admin user, permissions, demo tenant data)

## Rate Limiting

Webhook endpoints are rate-limited to 10 requests per 60 seconds per client:
- `/api/saudi/nafath/callback` — Nafath authentication callbacks
- `/api/saudi/zatca/webhook` — ZATCA asynchronous responses

## Monitoring

- Serilog structured logging to console and file
- OpenTelemetry-compatible metrics via `System.Diagnostics.Metrics`
- Custom Saudi Kit metrics: `saudikit.invoices.submitted`, `saudikit.invoices.cleared`, etc.
- Health check UI at `/health-ui`
