# NexusCore

[![CI](https://github.com/Alsairy/NexusCore/actions/workflows/ci.yml/badge.svg)](https://github.com/Alsairy/NexusCore/actions/workflows/ci.yml)
![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)
![Angular 20](https://img.shields.io/badge/Angular-20-DD0031)
![ABP Framework 10](https://img.shields.io/badge/ABP_Framework-10.0.2-4A90D9)
![License](https://img.shields.io/badge/license-MIT-green)

A multi-tenant Saudi Arabia compliance platform built on [ABP Framework](https://abp.io). Provides ZATCA e-invoicing (Fatoora), Nafath national identity SSO, approval workflows, and Hijri calendar — ready for production deployment.

---

## Features

**ZATCA E-Invoicing**
- Full invoice lifecycle: Draft → Validate → Submit → Cleared/Reported
- Standard (B2B clearance) and Simplified (B2C reporting) invoice types
- Credit notes and debit notes with original invoice reference
- UBL 2.1 XML generation compliant with ZATCA specifications
- X.509 digital signature with SHA-256 hashing
- TLV-encoded QR codes (scannable, Base64)
- PDF export with Arabic text support
- Seller and certificate management per tenant
- Webhook receiver for async ZATCA responses (HMAC-SHA256 validated)

**Nafath SSO**
- National ID authentication via Elm's Nafath service
- Random number challenge flow with polling
- Callback endpoint for async status updates
- Identity linking/unlinking per user account

**Approval Workflows**
- Task-based approval engine with role assignment
- Approve/reject with comments
- Delegation with date-based expiry and auto-routing

**Hijri Calendar**
- Server-side Gregorian ↔ Hijri conversion
- Dual calendar picker component (both calendars side by side)
- Hijri date pipe for templates
- Client-side fallback via `Intl.DateTimeFormat`

**Platform**
- Multi-tenant with per-tenant settings and data isolation
- OpenTelemetry metrics and distributed tracing
- Polly resilience (retry, circuit breaker, timeout) on all external API calls
- Rate limiting on anonymous endpoints
- Health checks for ZATCA and Nafath APIs
- Docker Compose deployment
- GitHub Actions CI/CD with automated Docker image publishing

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Angular 20 (SPA)                        │
│  ┌──────────┐ ┌──────────┐ ┌─────────┐ ┌────────────────┐  │
│  │  ZATCA   │ │  Nafath  │ │Workflows│ │ Hijri Calendar │  │
│  │ Invoices │ │   SSO    │ │  Inbox  │ │    Picker      │  │
│  └────┬─────┘ └────┬─────┘ └────┬────┘ └───────┬────────┘  │
│       │             │            │              │            │
│       └─────────────┴────────────┴──────────────┘            │
│                         REST API                             │
└────────────────────────────┬────────────────────────────────┘
                             │ HTTPS :44333
┌────────────────────────────┴────────────────────────────────┐
│                 NexusCore.HttpApi.Host                       │
│  Rate Limiting │ Health Checks │ OTel │ Auth │ CORS         │
├─────────────────────────────────────────────────────────────┤
│                 NexusCore.HttpApi                            │
│  ZATCA Webhook Controller │ Nafath Callback Controller       │
├─────────────────────────────────────────────────────────────┤
│               NexusCore.Application                         │
│  Invoice │ Seller │ Certificate │ Nafath │ Approval │ Hijri │
│  Polly Resilience (HttpClientFactory + retry/CB/timeout)    │
├─────────────────────────────────────────────────────────────┤
│                 NexusCore.Domain                             │
│  Entities │ Domain Services │ Validators │ XML Builder │ QR │
├─────────────────────────────────────────────────────────────┤
│             NexusCore.EntityFrameworkCore                    │
│  DbContext │ Repositories │ Migrations │ Data Seeding        │
├─────────────────────────────────────────────────────────────┤
│                   SQL Server 2022                            │
│  8 Saudi tables │ ABP system tables │ Multi-tenant           │
└─────────────────────────────────────────────────────────────┘
                             │
              ┌──────────────┼──────────────┐
              ▼              ▼              ▼
        ┌──────────┐  ┌──────────┐  ┌────────────┐
        │ ZATCA    │  │ Nafath   │  │ OpenIddict  │
        │ Fatoora  │  │ (Elm)    │  │ Auth Server │
        │ API      │  │ API      │  │             │
        └──────────┘  └──────────┘  └────────────┘
```

---

## Tech Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Backend Framework | ASP.NET Core + ABP Framework | .NET 10 / ABP 10.0.2 |
| Frontend | Angular + ABP Angular UI | Angular 20 / LeptonX Lite |
| Database | SQL Server | 2022 |
| ORM | Entity Framework Core | 10.0 |
| Auth | OpenIddict (via ABP) | — |
| Resilience | Microsoft.Extensions.Http.Resilience (Polly v8) | 9.4.0 |
| Observability | System.Diagnostics (OpenTelemetry-compatible) | Built-in |
| Containerization | Docker + Docker Compose | — |
| CI/CD | GitHub Actions | — |
| Package Manager | NuGet (.NET) / npm (Angular) | — |

---

## Project Structure

```
NexusCore/
├── src/
│   ├── NexusCore.Domain.Shared/        # Shared constants, enums, localization
│   ├── NexusCore.Domain/               # Entities, domain services, validators
│   │   └── Saudi/                      # 18 files: entities, services, QR, XML, signing
│   ├── NexusCore.Application.Contracts/ # Interfaces, DTOs, permissions
│   ├── NexusCore.Application/          # App services, API clients (ZATCA, Nafath)
│   ├── NexusCore.EntityFrameworkCore/  # DbContext, repositories, migrations
│   ├── NexusCore.HttpApi/              # Controllers (webhooks, callbacks)
│   ├── NexusCore.HttpApi.Host/         # Startup, config, health checks, metrics
│   ├── NexusCore.HttpApi.Client/       # Client-side proxy generation
│   └── NexusCore.DbMigrator/          # Database migration console app
├── angular/
│   └── src/app/saudi/                  # 25 files: components, services, pipes
│       ├── zatca/                      # Invoice list/detail/form, seller settings, QR, PDF
│       ├── nafath/                     # Login, identity linking
│       ├── workflows/                  # Approval inbox, delegations, designer
│       ├── hijri-calendar/             # Calendar component
│       ├── settings/                   # ZATCA/Nafath settings, certificate management
│       └── shared/                     # Hijri pipe, dual calendar picker, RTL styles
├── test/
│   ├── NexusCore.Domain.Tests/         # Domain logic tests (38 tests)
│   ├── NexusCore.Application.Tests/    # App service tests
│   ├── NexusCore.EntityFrameworkCore.Tests/ # EF Core integration tests (91 tests)
│   └── NexusCore.TestBase/            # Shared test data seeder, base classes
├── docs/
│   ├── ROADMAP.md                      # Post-launch roadmap (Phases 9–20)
│   ├── deployment-guide.md             # Production deployment guide
│   ├── saudi-kit-operations.md         # ZATCA/Nafath operations guide
│   ├── security-review.md             # Security review and recommendations
│   └── adr/                           # Architecture Decision Records
├── .github/workflows/
│   ├── ci.yml                         # Build + test on push/PR
│   └── release.yml                    # Docker image publish on tag
├── docker-compose.yml                 # Full stack: API + Angular + SQL Server
└── docker-compose.override.yml        # Dev overrides
```

---

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 22+](https://nodejs.org/)
- [SQL Server 2022](https://www.microsoft.com/sql-server) (or Docker)

### Option 1: Docker Compose (Recommended)

```bash
git clone https://github.com/Alsairy/NexusCore.git
cd NexusCore
docker compose up -d
```

| Service | URL |
|---------|-----|
| Angular UI | http://localhost:4200 |
| API (Swagger) | https://localhost:44333/swagger |
| SQL Server | localhost:1433 |

Default credentials: `admin` / `1q2w3E*`

### Option 2: Manual Setup

**1. Clone and restore**
```bash
git clone https://github.com/Alsairy/NexusCore.git
cd NexusCore
dotnet restore
cd angular && npm ci --legacy-peer-deps && cd ..
```

**2. Configure the database**

Update the connection string in `src/NexusCore.HttpApi.Host/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=NexusCore;User Id=SA;Password=YourPassword;TrustServerCertificate=true"
  }
}
```

**3. Run migrations**
```bash
dotnet run --project src/NexusCore.DbMigrator
```

**4. Start the API**
```bash
dotnet run --project src/NexusCore.HttpApi.Host
```

**5. Start the Angular app** (in a separate terminal)
```bash
cd angular
npm start
```

**6. Open the app**

Navigate to http://localhost:4200 and log in with `admin` / `1q2w3E*`.

---

## Saudi Kit Modules

### ZATCA E-Invoicing

Navigate to **ZATCA > Invoices** to manage invoices. See the [Operations Guide](docs/saudi-kit-operations.md) for the full onboarding workflow:

1. Configure ZATCA API settings (sandbox/simulation/production)
2. Create a seller with Arabic name and VAT number
3. Upload the ZATCA-issued certificate
4. Create, validate, and submit invoices

Supports all ZATCA invoice types: Standard (B2B), Simplified (B2C), Credit Note, and Debit Note.

### Nafath SSO

Navigate to **Nafath** to authenticate users via Saudi national identity:

1. Configure Nafath API credentials (from Elm)
2. Users enter their 10-digit National ID
3. Users approve in the Nafath mobile app
4. Identity is linked to their NexusCore account

### Approval Workflows

Navigate to **Workflows > Inbox** to manage approvals:

- View pending tasks assigned to you (or delegated to you)
- Approve or reject with comments
- Set up delegation for absence coverage

### Hijri Calendar

Available throughout the Saudi module:

- Gregorian ↔ Hijri date conversion
- Dual calendar picker for date inputs
- Hijri date pipe for display formatting

---

## Testing

```bash
# Run all backend tests
dotnet test

# Run domain tests only
dotnet test test/NexusCore.Domain.Tests/

# Run EF Core integration tests only
dotnet test test/NexusCore.EntityFrameworkCore.Tests/

# Run Angular tests
cd angular && ng test --watch=false
```

**Test coverage:** 129+ backend tests across domain, application, and EF Core layers.

---

## API Documentation

When the API is running, Swagger UI is available at:

```
https://localhost:44333/swagger
```

All Saudi Kit endpoints are under:
- `/api/app/zatca-invoice/*` — Invoice CRUD + submit + validate
- `/api/app/zatca-seller/*` — Seller CRUD + set default
- `/api/app/zatca-certificate/*` — Certificate CRUD + activate
- `/api/app/nafath/*` — Nafath auth + identity linking
- `/api/app/approval-inbox/*` — Approval tasks
- `/api/app/delegation/*` — Delegation CRUD
- `/api/app/hijri-calendar/*` — Date conversion
- `/api/app/saudi-settings/*` — ZATCA/Nafath settings management

Anonymous endpoints (webhooks/callbacks):
- `POST /api/saudi/zatca/webhook` — ZATCA async responses
- `POST /api/saudi/nafath/callback` — Nafath status callbacks

---

## Health Checks

```
GET /health-status
```

Returns status of:
- SQL Server connectivity
- ZATCA API reachability
- Nafath API reachability

---

## Deployment

See the [Deployment Guide](docs/deployment-guide.md) for:
- Production `appsettings.json` configuration
- Docker Compose deployment
- HTTPS/TLS setup
- OpenIddict certificate configuration

See the [Security Review](docs/security-review.md) for:
- Anonymous endpoint protections
- Input validation coverage
- Encrypted settings
- Rate limiting policies
- Resilience policies (retry, circuit breaker)

---

## Documentation

| Document | Description |
|----------|-------------|
| [Deployment Guide](docs/deployment-guide.md) | Production deployment instructions |
| [Operations Guide](docs/saudi-kit-operations.md) | ZATCA onboarding, Nafath setup, troubleshooting |
| [Security Review](docs/security-review.md) | Security analysis and recommendations |
| [Roadmap](docs/ROADMAP.md) | Post-launch roadmap (Phases 9–20) |
| [ADR-001](docs/adr/001-abp-framework-selection.md) | Why ABP Framework was chosen |
| [ADR-002](docs/adr/002-zatca-xml-builder-approach.md) | ZATCA XML generation approach |
| [ADR-003](docs/adr/003-nafath-polling-vs-webhook.md) | Nafath polling vs webhook strategy |

---

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Commit your changes following [Conventional Commits](https://www.conventionalcommits.org/)
4. Push to the branch: `git push origin feature/my-feature`
5. Open a Pull Request against `main`

Please ensure:
- All existing tests pass (`dotnet test`)
- New features include tests
- Angular builds without errors (`cd angular && ng build`)

---

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
