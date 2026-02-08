# ADR-001: ABP Framework Selection

## Status

Accepted

## Date

2024-12-01

## Context

NexusCore requires a full-stack .NET framework to build a multi-tenant SaaS platform with Saudi Arabia compliance features (ZATCA e-invoicing, Nafath SSO). The platform needs:

- **Multi-tenancy** with per-tenant data isolation and settings
- **Modular architecture** to organize Saudi-specific features separately from core platform
- **Identity and access management** with OpenID Connect, role-based permissions, and tenant-scoped authorization
- **Entity Framework Core** integration with migration tooling
- **Angular frontend** support with UI components, theming, and localization
- **Audit logging** for compliance requirements
- **Setting management** with encryption support for sensitive credentials (ZATCA CSID, Nafath AppKey)

Alternatives evaluated:

1. **Plain ASP.NET Core** — Full control, but requires building multi-tenancy, audit logging, permission management, setting management, and localization from scratch.
2. **ABP Framework** — Opinionated DDD framework with all required infrastructure built-in. Open-source (LGPL) with commercial UI themes.
3. **Orchard Core** — CMS-oriented, strong multi-tenancy but less focused on business application patterns (DDD, app services, DTOs).
4. **ASP.NET Boilerplate** — Predecessor to ABP, mature but targeting older .NET versions and lacking modern features.

## Decision

We chose **ABP Framework (v10.0.2)** as the application foundation.

Key factors:

- **Multi-tenancy is first-class** — `IMultiTenant` interface on entities, automatic tenant filtering via EF Core global query filters, `ICurrentTenant` for runtime context. This saved weeks of infrastructure work.
- **Setting management with encryption** — `ISettingManager.SetForCurrentTenantAsync()` provides per-tenant encrypted storage, critical for ZATCA credentials and Nafath API keys.
- **Permission system** — Declarative permission definitions, role-based and user-based grants, Angular integration via `@abp/ng.core`. Maps directly to ZATCA permission model (view invoices, submit invoices, manage certificates, etc.).
- **Audit logging** — Automatic audit trail on all authenticated API calls. Required for Saudi regulatory compliance.
- **Angular integration** — `@abp/ng.core` provides `RestService` for API calls, `LocalizationService` for i18n, `PermissionService` for UI guards, and `ConfirmationService`/`ToasterService` for UX patterns.
- **DDD building blocks** — `AggregateRoot`, `Entity`, `DomainService`, `ApplicationService` enforce clean separation between Saudi domain logic (XML building, QR generation, invoice validation) and infrastructure.
- **Code generation** — ABP CLI generates Angular proxies from .NET API endpoints, reducing boilerplate.

## Consequences

**Positive:**
- Multi-tenancy, permissions, audit logging, and settings management required zero custom infrastructure code
- Consistent patterns across all Saudi modules (same base classes, same DTO mapping, same permission model)
- Angular proxy generation keeps frontend in sync with backend APIs
- LeptonX Lite theme provides professional UI with RTL support
- Large community and documentation for common patterns

**Negative:**
- Framework coupling — migrating away from ABP would require rewriting infrastructure layers
- ABP version upgrades can be complex when they introduce breaking changes
- Learning curve for developers unfamiliar with ABP's conventions (module system, dependency injection patterns, virtual file system)
- Commercial LeptonX theme required for advanced UI features (free LeptonX Lite has limitations)
- Package size — ABP brings many transitive NuGet dependencies
