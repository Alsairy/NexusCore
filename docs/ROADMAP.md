# NexusCore Saudi Kit — Post-Launch Roadmap

> **Status:** Phases 1–8 (foundation) are complete and merged to `main`.
> This roadmap covers everything needed to go from "code complete" to "commercially deployed and battle-tested."

---

## Table of Contents

- [Phase 9: README & Project Documentation](#phase-9-readme--project-documentation)
- [Phase 10: Angular Unit & Component Tests](#phase-10-angular-unit--component-tests)
- [Phase 11: Application Service Test Coverage](#phase-11-application-service-test-coverage)
- [Phase 12: Arabic Localization & Full RTL Support](#phase-12-arabic-localization--full-rtl-support)
- [Phase 13: ZATCA Sandbox Integration Testing](#phase-13-zatca-sandbox-integration-testing)
- [Phase 14: Nafath Sandbox Integration Testing](#phase-14-nafath-sandbox-integration-testing)
- [Phase 15: Multi-Tenant Onboarding Wizard](#phase-15-multi-tenant-onboarding-wizard)
- [Phase 16: Dashboard & Analytics](#phase-16-dashboard--analytics)
- [Phase 17: Audit Log Viewer](#phase-17-audit-log-viewer)
- [Phase 18: End-to-End Tests (Playwright)](#phase-18-end-to-end-tests-playwright)
- [Phase 19: Performance & Caching](#phase-19-performance--caching)
- [Phase 20: Release v1.0.0 & Changelog](#phase-20-release-v100--changelog)
- [Dependency Graph](#dependency-graph)
- [Totals](#totals)

---

## Phase 9: README & Project Documentation

**Priority: HIGHEST — first thing visitors see on GitHub**
**~4 files (1 rewrite, 3 new)**

### 9A. Rewrite Root README.md

| Action | File |
|--------|------|
| Rewrite | `README.md` |

Replace the generic ABP template README with a proper project README:

- **Hero section**: Project name, one-line description, badges (build status, .NET version, Angular version, license)
- **Features list**: ZATCA E-Invoicing, Nafath SSO, Approval Workflows, Hijri Calendar, multi-tenant
- **Architecture diagram**: ASCII or Mermaid diagram showing layered architecture (Angular → HttpApi.Host → Application → Domain → EF Core → SQL Server)
- **Tech stack table**: .NET 10, ABP 10.0.2, Angular 20, SQL Server, Docker, GitHub Actions
- **Quick start**:
  1. Prerequisites (`.NET 10 SDK`, `Node 22`, `SQL Server` or `Docker`)
  2. Clone the repo
  3. `dotnet restore` / `npm ci --legacy-peer-deps`
  4. Run migrations (`NexusCore.DbMigrator`)
  5. Start API (`dotnet run --project src/NexusCore.HttpApi.Host`)
  6. Start Angular (`cd angular && npm start`)
  7. Open `http://localhost:4200`
- **Docker quick start**: `docker compose up` — 3 commands to full stack
- **Project structure**: tree view of `src/` and `test/` with one-line descriptions per project
- **Saudi Kit modules**: brief description of each module with link to operations guide
- **API documentation**: link to Swagger (`/swagger`)
- **Testing**: how to run all tests (`dotnet test`, `ng test`)
- **Deployment**: link to `docs/deployment-guide.md`
- **Security**: link to `docs/security-review.md`
- **Contributing**: basic contributing guidelines
- **License**: license badge + link

### 9B. Architecture Decision Records

| Action | File |
|--------|------|
| Create | `docs/adr/001-abp-framework-selection.md` |
| Create | `docs/adr/002-zatca-xml-builder-approach.md` |
| Create | `docs/adr/003-nafath-polling-vs-webhook.md` |

Each ADR follows the standard format:
- **Title**: Short decision title
- **Status**: Accepted
- **Context**: What problem needed solving
- **Decision**: What was chosen and why
- **Consequences**: Trade-offs accepted

### Verification
- README renders correctly on GitHub with all badges and links working
- Architecture diagram displays in Mermaid preview

---

## Phase 10: Angular Unit & Component Tests

**Priority: HIGH — zero frontend test coverage currently**
**~18 new files | Depends on: None**

### 10A. Test Infrastructure Setup

| Action | File |
|--------|------|
| Create | `angular/src/app/saudi/testing/saudi-test-helpers.ts` |
| Create | `angular/src/app/saudi/testing/mock-services.ts` |

**`saudi-test-helpers.ts`:**
- `createMockInvoice(overrides?)` — returns a `ZatcaInvoiceDto` with sensible defaults
- `createMockSeller(overrides?)` — returns a `ZatcaSellerDto`
- `createMockCertificate(overrides?)` — returns a `ZatcaCertificateDto`
- `createMockNafathLink(overrides?)` — returns a `NafathUserLinkDto`
- `createMockApprovalTask(overrides?)` — returns an `ApprovalTaskDto`
- `createMockDelegation(overrides?)` — returns a `DelegationDto`
- `createMockHijriDate()` — returns today as `HijriDateDto`

**`mock-services.ts`:**
- `MockInvoiceService` — jasmine spies for all methods, returns `of(mockData)`
- `MockSellerService` — same pattern
- `MockCertificateService` — same pattern
- `MockNafathService` — same pattern
- `MockApprovalService` — same pattern
- `MockDelegationService` — same pattern
- `MockHijriCalendarService` — same pattern
- `MockSaudiSettingsService` — same pattern
- `MockToasterService` — captures toast calls for assertion
- `MockConfirmationService` — returns `of(Confirmation.Status.confirm)` by default

### 10B. Component Tests (14 files)

Each test file follows this pattern:
1. `TestBed.configureTestingModule()` with standalone component + mock service providers
2. `fixture = TestBed.createComponent(ComponentClass)`
3. Test: component creates successfully
4. Test: renders expected elements in default state
5. Test: loading state shows spinner
6. Test: error state shows error message
7. Test: user interactions trigger correct service calls
8. Test: data binding displays correct values

| Test File | Component | Key Test Cases |
|-----------|-----------|----------------|
| `zatca/invoice-list/invoice-list.component.spec.ts` | InvoiceListComponent | Renders table rows from mock data; filter by status calls `getList` with correct params; pagination emits correct page; "New Invoice" button navigates to form; empty state shows message |
| `zatca/invoice-detail/invoice-detail.component.spec.ts` | InvoiceDetailComponent | Loads invoice by route param; displays seller/buyer info; shows line items table; calculates totals correctly; "Submit to ZATCA" button calls `submit()`; QR code component receives correct data; "Download PDF" calls pdf service; shows status badge with correct class |
| `zatca/invoice-form/invoice-form.component.spec.ts` | InvoiceFormComponent | Form validation — required fields show errors; seller dropdown populated from service; add/remove line items; line item calculations (qty × price = amount); VAT calculation (15%); form submit calls `create()` for new, `update()` for edit; cancel navigates back |
| `zatca/seller-settings/seller-settings.component.spec.ts` | SellerSettingsComponent | Lists sellers from API; create form validates VAT (15 digits); edit populates form; delete shows confirmation; "Set Default" calls correct API; Arabic name field accepts Arabic characters |
| `zatca/components/zatca-qr-code.component.spec.ts` | ZatcaQrCodeComponent | Renders canvas element; no render when data is empty; size input controls canvas dimensions |
| `nafath/nafath-login/nafath-login.component.spec.ts` | NafathLoginComponent | National ID input validates 10 digits; submit calls `initiateLogin()`; displays random number after initiation; polling starts after initiation; timeout shows expired message; success redirects/shows success |
| `nafath/nafath-link-identity/nafath-link-identity.component.spec.ts` | NafathLinkIdentityComponent | Shows "Link Identity" when no link exists; shows linked identity when link exists; unlink shows confirmation; link initiates Nafath flow |
| `workflows/approval-inbox/approval-inbox.component.spec.ts` | ApprovalInboxComponent | Renders task list; approve button calls `approve()` with comment; reject button calls `reject()` with comment; empty inbox shows message; task detail expands on click |
| `workflows/delegations/delegations.component.spec.ts` | DelegationsComponent | Lists active delegations; create form validates dates (end > start); delete shows confirmation; expired delegations show visual indicator |
| `workflows/workflow-designer/workflow-designer.component.spec.ts` | WorkflowDesignerComponent | Component creates; renders design surface (basic smoke test) |
| `hijri-calendar/hijri-calendar.component.spec.ts` | HijriCalendarComponent | Displays today's Hijri date; conversion input/output works; month name displayed correctly |
| `settings/saudi-settings.component.spec.ts` | SaudiSettingsComponent | Loads ZATCA settings on init; loads Nafath settings on tab switch; save calls update with form values; password fields toggle visibility; environment dropdown has 3 options |
| `settings/certificate-management.component.spec.ts` | CertificateManagementComponent | Seller dropdown populated; certificates load when seller selected; create form validates required fields; activate calls API; delete shows confirmation |
| `shared/hijri-date.pipe.spec.ts` | HijriDatePipe | Transforms Gregorian date to Hijri string; handles null/undefined input; handles invalid date input |

### 10C. Service Tests (2 files)

| Test File | Service | Key Test Cases |
|-----------|---------|----------------|
| `zatca/services/invoice-pdf.service.spec.ts` | InvoicePdfService | `generate()` calls jsPDF methods; handles invoice with no QR code; handles invoice with long Arabic seller name |
| `shared/hijri-date.pipe.spec.ts` | HijriDatePipe | Already covered above |

### Verification
- `ng test --watch=false --browsers=ChromeHeadless` — all tests pass
- Coverage report generated: `ng test --code-coverage` — target: >80% line coverage for Saudi module
- Add to CI: update `.github/workflows/ci.yml` to run `ng test --watch=false --browsers=ChromeHeadless`

---

## Phase 11: Application Service Test Coverage

**Priority: HIGH — only 3 app service test files exist, need 6 more**
**~6 new files | Depends on: None (existing test seeders from Phase 2)**

### Current State

Existing application test files:
- `NexusCoreApplicationTestBase.cs` (base class)
- `NexusCoreApplicationTestModule.cs` (module)
- `NexusCoreSampleAppServiceTests.cs` (sample/template)

**Missing test files for all 6 Saudi app services.**

### 11A. Application Service Tests

| File | App Service | Test Cases |
|------|-------------|------------|
| `test/NexusCore.Application.Tests/Saudi/HijriCalendar/HijriCalendarAppServiceTests.cs` | HijriCalendarAppService | `GetTodayHijriAsync` returns valid date with all fields; `ConvertToHijriAsync` for known date (2024-01-01 → expected Hijri); `ConvertToGregorianAsync` round-trip matches; `GetMonthInfoAsync` returns 29 or 30 days; invalid date input throws `UserFriendlyException` |
| `test/NexusCore.Application.Tests/Saudi/Zatca/ZatcaSellerAppServiceTests.cs` | ZatcaSellerAppService | `GetListAsync` returns seeded seller; `GetAsync` returns correct seller by ID; `CreateAsync` with valid input creates seller; `CreateAsync` with duplicate VAT throws; `UpdateAsync` modifies fields; `DeleteAsync` removes seller; `SetAsDefaultAsync` deactivates others; `GetDefaultAsync` returns the default seller |
| `test/NexusCore.Application.Tests/Saudi/Zatca/ZatcaInvoiceAppServiceTests.cs` | ZatcaInvoiceAppService | `GetListAsync` returns seeded invoices; `GetListAsync` with status filter works; `GetListAsync` with date range filter works; `GetAsync` includes line items; `CreateAsync` with valid input creates invoice + lines; `CreateAsync` calculates totals correctly (subtotal, VAT, total); `UpdateAsync` only works for Draft status; `UpdateAsync` on non-Draft throws `BusinessException`; `DeleteAsync` only works for Draft; `ValidateAsync` on valid invoice returns success; `ValidateAsync` on invalid invoice returns errors |
| `test/NexusCore.Application.Tests/Saudi/Zatca/ZatcaCertificateAppServiceTests.cs` | ZatcaCertificateAppService | `GetListAsync` by seller returns seeded certificate; `CreateAsync` with valid input creates certificate; `DeleteAsync` removes certificate; `ActivateAsync` activates and deactivates all other certificates for that seller; `DeactivateAsync` sets IsActive to false; `GetAsync` returns correct fields |
| `test/NexusCore.Application.Tests/Saudi/Nafath/NafathAppServiceTests.cs` | NafathAppService | `GetMyLinkAsync` returns seeded link for current user; `GetMyLinkAsync` returns null for user without link; `CheckStatusAsync` with valid transaction returns status; `CheckStatusAsync` with expired transaction returns expired |
| `test/NexusCore.Application.Tests/Saudi/Workflows/ApprovalInboxAppServiceTests.cs` | ApprovalInboxAppService | `GetMyTasksAsync` returns tasks assigned to current user; `GetMyTasksAsync` includes tasks delegated to current user; `ApproveAsync` changes status to Approved; `ApproveAsync` with comment saves comment; `RejectAsync` changes status to Rejected; `ApproveAsync` by unauthorized user throws `BusinessException`; `ApproveAsync` on already-completed task throws |

### 11B. Delegation App Service Tests

| File | App Service | Test Cases |
|------|-------------|------------|
| `test/NexusCore.Application.Tests/Saudi/Workflows/DelegationAppServiceTests.cs` | DelegationAppService | `GetListAsync` returns seeded delegations; `CreateAsync` with valid dates creates delegation; `CreateAsync` with end date before start date throws; `DeleteAsync` removes delegation; `GetMyActiveDelegationsAsync` returns only active (date range includes today) |

### Test Patterns

All tests follow ABP conventions:
```csharp
public class ZatcaSellerAppServiceTests : NexusCoreApplicationTestBase
{
    private readonly IZatcaSellerAppService _sellerAppService;

    public ZatcaSellerAppServiceTests()
    {
        _sellerAppService = GetRequiredService<IZatcaSellerAppService>();
    }

    [Fact]
    public async Task Should_Get_List_Of_Sellers()
    {
        var result = await _sellerAppService.GetListAsync(new GetSellerListInput());
        result.TotalCount.ShouldBeGreaterThan(0);
        result.Items.ShouldContain(s => s.VatRegistrationNumber == NexusCoreTestData.SellerVatNumber);
    }
}
```

### Verification
- `dotnet test test/NexusCore.Application.Tests/` — all pass (existing + new)
- Total application test count target: **40+ test cases**

---

## Phase 12: Arabic Localization & Full RTL Support

**Priority: HIGH — essential for Saudi market**
**~14 files (10 new, 4 modified)**

### 12A. Backend Localization Resources

| Action | File |
|--------|------|
| Modify | `src/NexusCore.Domain.Shared/Localization/NexusCore/en.json` |
| Create | `src/NexusCore.Domain.Shared/Localization/NexusCore/ar.json` |

**`en.json` additions** (~120 new keys):
```json
{
  "Saudi:Zatca:Invoices": "ZATCA Invoices",
  "Saudi:Zatca:InvoiceNumber": "Invoice Number",
  "Saudi:Zatca:InvoiceDate": "Invoice Date",
  "Saudi:Zatca:InvoiceType": "Invoice Type",
  "Saudi:Zatca:InvoiceType:Standard": "Standard (B2B)",
  "Saudi:Zatca:InvoiceType:Simplified": "Simplified (B2C)",
  "Saudi:Zatca:InvoiceType:CreditNote": "Credit Note",
  "Saudi:Zatca:InvoiceType:DebitNote": "Debit Note",
  "Saudi:Zatca:Status:Draft": "Draft",
  "Saudi:Zatca:Status:Validated": "Validated",
  "Saudi:Zatca:Status:Reported": "Reported",
  "Saudi:Zatca:Status:Cleared": "Cleared",
  "Saudi:Zatca:Status:Rejected": "Rejected",
  "Saudi:Zatca:Status:Archived": "Archived",
  "Saudi:Zatca:Sellers": "Sellers",
  "Saudi:Zatca:SellerName": "Seller Name (Arabic)",
  "Saudi:Zatca:VatNumber": "VAT Registration Number",
  "Saudi:Zatca:Certificates": "Certificates",
  "Saudi:Zatca:SubmitToZatca": "Submit to ZATCA",
  "Saudi:Zatca:Validate": "Validate Invoice",
  "Saudi:Nafath:Authentication": "Nafath Authentication",
  "Saudi:Nafath:NationalId": "National ID",
  "Saudi:Nafath:EnterNationalId": "Enter your 10-digit National ID",
  "Saudi:Nafath:WaitingForApproval": "Waiting for approval in Nafath app...",
  "Saudi:Nafath:SelectNumber": "Select number {0} in your Nafath app",
  "Saudi:Nafath:LinkIdentity": "Link Nafath Identity",
  "Saudi:Nafath:UnlinkIdentity": "Unlink Identity",
  "Saudi:Workflows:ApprovalInbox": "Approval Inbox",
  "Saudi:Workflows:Approve": "Approve",
  "Saudi:Workflows:Reject": "Reject",
  "Saudi:Workflows:Delegations": "Delegations",
  "Saudi:HijriCalendar": "Hijri Calendar",
  "Saudi:Settings": "Saudi Settings"
}
```

**`ar.json`** (complete Arabic translation of all keys):
```json
{
  "Saudi:Zatca:Invoices": "فواتير زاتكا",
  "Saudi:Zatca:InvoiceNumber": "رقم الفاتورة",
  "Saudi:Zatca:InvoiceDate": "تاريخ الفاتورة",
  "Saudi:Zatca:InvoiceType": "نوع الفاتورة",
  "Saudi:Zatca:InvoiceType:Standard": "فاتورة ضريبية (B2B)",
  "Saudi:Zatca:InvoiceType:Simplified": "فاتورة مبسطة (B2C)",
  "Saudi:Zatca:InvoiceType:CreditNote": "إشعار دائن",
  "Saudi:Zatca:InvoiceType:DebitNote": "إشعار مدين",
  "Saudi:Zatca:Status:Draft": "مسودة",
  "Saudi:Zatca:Status:Validated": "تم التحقق",
  "Saudi:Zatca:Status:Reported": "تم الإبلاغ",
  "Saudi:Zatca:Status:Cleared": "تم الاعتماد",
  "Saudi:Zatca:Status:Rejected": "مرفوض",
  "Saudi:Zatca:Status:Archived": "مؤرشف",
  "Saudi:Zatca:Sellers": "البائعون",
  "Saudi:Zatca:SellerName": "اسم البائع (عربي)",
  "Saudi:Zatca:VatNumber": "رقم التسجيل الضريبي",
  "Saudi:Zatca:Certificates": "الشهادات",
  "Saudi:Zatca:SubmitToZatca": "إرسال إلى زاتكا",
  "Saudi:Zatca:Validate": "التحقق من الفاتورة",
  "Saudi:Nafath:Authentication": "مصادقة نفاذ",
  "Saudi:Nafath:NationalId": "رقم الهوية الوطنية",
  "Saudi:Nafath:EnterNationalId": "أدخل رقم الهوية المكون من 10 أرقام",
  "Saudi:Nafath:WaitingForApproval": "بانتظار الموافقة في تطبيق نفاذ...",
  "Saudi:Nafath:SelectNumber": "اختر الرقم {0} في تطبيق نفاذ",
  "Saudi:Nafath:LinkIdentity": "ربط هوية نفاذ",
  "Saudi:Nafath:UnlinkIdentity": "إلغاء ربط الهوية",
  "Saudi:Workflows:ApprovalInbox": "صندوق الموافقات",
  "Saudi:Workflows:Approve": "موافقة",
  "Saudi:Workflows:Reject": "رفض",
  "Saudi:Workflows:Delegations": "التفويضات",
  "Saudi:HijriCalendar": "التقويم الهجري",
  "Saudi:Settings": "الإعدادات السعودية"
}
```

### 12B. Angular Localization Infrastructure

| Action | File |
|--------|------|
| Create | `angular/src/assets/i18n/en.json` |
| Create | `angular/src/assets/i18n/ar.json` |
| Modify | `angular/src/app/saudi/saudi.routes.ts` — add localization imports |

ABP Angular uses `@abp/ng.core` `LocalizationService`. All Saudi component templates must replace hardcoded strings with `{{ '::KeyName' | abpLocalization }}` pipe.

### 12C. RTL Layout Support

| Action | File |
|--------|------|
| Modify | `angular/src/app/saudi/shared/rtl-overrides.scss` |
| Create | `angular/src/app/saudi/shared/directives/rtl.directive.ts` |

**`rtl-overrides.scss` expansion:**
```scss
[dir="rtl"] {
  // Table alignment
  .saudi-table th,
  .saudi-table td {
    text-align: right;
  }

  // Form labels
  .saudi-form .form-label {
    text-align: right;
  }

  // Status badges — flip margin direction
  .badge + .badge {
    margin-right: 0.25rem;
    margin-left: 0;
  }

  // Invoice detail — flip layout
  .invoice-header {
    flex-direction: row-reverse;
  }

  // Navigation arrows
  .pagination .page-link {
    .bi-chevron-left::before { content: "\F285"; } // chevron-right
    .bi-chevron-right::before { content: "\F284"; } // chevron-left
  }

  // QR code alignment
  .qr-section {
    text-align: left; // QR goes to left in RTL
  }
}
```

**`rtl.directive.ts`:**
- Standalone directive that auto-applies `dir="rtl"` and `lang="ar"` when Arabic locale is active
- Listens to `LocalizationService.currentLang$`
- Applied to Saudi module root containers

### 12D. Component Template Updates (8 files modified)

Every component template replaces hardcoded English strings:

| File | Changes |
|------|---------|
| `invoice-list.component.ts` | All column headers, button labels, status labels, empty state message → localization pipe |
| `invoice-detail.component.ts` | Section headings, field labels, button labels, status display |
| `invoice-form.component.ts` | Form labels, validation messages, button labels, placeholders |
| `seller-settings.component.ts` | Column headers, form labels, button labels |
| `nafath-login.component.ts` | Instructions, input placeholder, button labels, status messages |
| `nafath-link-identity.component.ts` | Section headings, button labels, status display |
| `approval-inbox.component.ts` | Column headers, button labels, empty state, comment placeholder |
| `delegations.component.ts` | Column headers, form labels, date labels |

### Verification
- Switch browser language to Arabic → all Saudi module UI renders in Arabic with RTL layout
- No layout breakage in RTL mode
- English mode remains unchanged
- All hardcoded strings eliminated from Saudi components

---

## Phase 13: ZATCA Sandbox Integration Testing

**Priority: HIGH — validates the integration actually works with ZATCA**
**~6 files (4 new, 2 modified) | Depends on: Phase 11**

### 13A. ZATCA Sandbox Configuration

| Action | File |
|--------|------|
| Create | `test/NexusCore.Integration.Tests/NexusCore.Integration.Tests.csproj` |
| Create | `test/NexusCore.Integration.Tests/Saudi/Zatca/ZatcaSandboxTests.cs` |
| Create | `test/NexusCore.Integration.Tests/appsettings.test.json` |

**New test project:**
- References `NexusCore.Application` and `NexusCore.EntityFrameworkCore`
- Uses a real SQL Server (via Testcontainers) instead of in-memory
- Reads ZATCA sandbox credentials from `appsettings.test.json` or environment variables
- Marked with `[Trait("Category", "Integration")]` so they don't run in CI by default

**Test cases:**
```csharp
[Trait("Category", "Integration")]
public class ZatcaSandboxTests
{
    // 1. Generate compliance CSID
    [Fact] Task Should_Onboard_With_Compliance_Csid()

    // 2. Submit simplified invoice (reporting)
    [Fact] Task Should_Report_Simplified_Invoice()

    // 3. Submit standard invoice (clearance)
    [Fact] Task Should_Clear_Standard_Invoice()

    // 4. Submit credit note referencing original
    [Fact] Task Should_Report_Credit_Note()

    // 5. Handle validation errors from ZATCA
    [Fact] Task Should_Return_Validation_Errors_For_Invalid_Invoice()

    // 6. Verify QR code is returned for cleared invoice
    [Fact] Task Should_Receive_Qr_Code_After_Clearance()

    // 7. Verify XML structure matches ZATCA UBL 2.1 requirements
    [Fact] Task Should_Generate_Valid_Ubl21_Xml()
}
```

### 13B. ZATCA XML Compliance Validator

| Action | File |
|--------|------|
| Create | `test/NexusCore.Integration.Tests/Saudi/Zatca/ZatcaXmlComplianceTests.cs` |

**Tests that validate generated XML against ZATCA's published rules:**
- All mandatory UBL 2.1 namespaces present
- `cbc:ProfileID` matches invoice type (reporting vs clearance)
- `cbc:ID` contains valid UUID format
- `cac:AccountingSupplierParty` contains Arabic name
- `cac:TaxTotal` calculations match line items
- Digital signature placeholder present
- QR code TLV contains all 5 required tags (seller name, VAT number, timestamp, total, VAT total)

### 13C. CI Integration

| Action | File |
|--------|------|
| Modify | `.github/workflows/ci.yml` — add optional integration test job triggered by `[integration]` in commit message |

### Verification
- Run against ZATCA sandbox: `dotnet test test/NexusCore.Integration.Tests/ --filter Category=Integration`
- All 7+ tests pass against live sandbox
- XML compliance tests pass offline (no network needed)

---

## Phase 14: Nafath Sandbox Integration Testing

**Priority: MEDIUM-HIGH — validates Nafath integration**
**~3 new files | Depends on: Phase 13 (shared test project)**

### 14A. Nafath Sandbox Tests

| Action | File |
|--------|------|
| Create | `test/NexusCore.Integration.Tests/Saudi/Nafath/NafathSandboxTests.cs` |

**Test cases:**
```csharp
[Trait("Category", "Integration")]
public class NafathSandboxTests
{
    // 1. Initiate authentication request
    [Fact] Task Should_Initiate_Auth_Request_And_Receive_TransactionId()

    // 2. Check status returns valid response
    [Fact] Task Should_Check_Status_Returns_Waiting_Or_Completed()

    // 3. Handle expired transaction
    [Fact] Task Should_Handle_Expired_Transaction_Gracefully()

    // 4. Handle invalid National ID format
    [Fact] Task Should_Reject_Invalid_National_Id()

    // 5. Handle API timeout
    [Fact] Task Should_Timeout_After_Configured_Duration()
}
```

### 14B. Nafath Callback Simulation

| Action | File |
|--------|------|
| Create | `test/NexusCore.Integration.Tests/Saudi/Nafath/NafathCallbackTests.cs` |

**Test cases (using TestServer):**
```csharp
public class NafathCallbackTests
{
    // 1. Valid callback updates auth request status
    [Fact] Task Should_Process_Valid_Callback()

    // 2. Callback with unknown transaction returns 404
    [Fact] Task Should_Return_NotFound_For_Unknown_Transaction()

    // 3. Rate limiting returns 429 after 10 requests
    [Fact] Task Should_Rate_Limit_After_10_Requests()

    // 4. Invalid payload returns 400
    [Fact] Task Should_Return_BadRequest_For_Invalid_Payload()
}
```

### 14C. Webhook Security Tests

| Action | File |
|--------|------|
| Create | `test/NexusCore.Integration.Tests/Saudi/Zatca/ZatcaWebhookSecurityTests.cs` |

**Test cases (using TestServer):**
```csharp
public class ZatcaWebhookSecurityTests
{
    // 1. Valid HMAC signature accepted
    [Fact] Task Should_Accept_Valid_Hmac_Signature()

    // 2. Invalid signature rejected with 401
    [Fact] Task Should_Reject_Invalid_Hmac_Signature()

    // 3. Missing signature header rejected
    [Fact] Task Should_Reject_Missing_Signature_Header()

    // 4. Rate limiting returns 429
    [Fact] Task Should_Rate_Limit_Webhook_Requests()

    // 5. No secret configured → accepts all (dev mode)
    [Fact] Task Should_Accept_All_When_No_Secret_Configured()
}
```

### Verification
- Nafath sandbox tests pass: `dotnet test --filter Category=Integration&FullyQualifiedName~Nafath`
- Callback/webhook security tests pass: `dotnet test --filter FullyQualifiedName~Callback|Webhook`

---

## Phase 15: Multi-Tenant Onboarding Wizard

**Priority: MEDIUM — streamlines tenant setup**
**~12 files (10 new, 2 modified)**

### 15A. Backend — Onboarding Status Tracking

| Action | File |
|--------|------|
| Create | `src/NexusCore.Domain/Saudi/Onboarding/Entities/TenantOnboardingStatus.cs` |
| Create | `src/NexusCore.Domain/Saudi/Onboarding/ITenantOnboardingRepository.cs` |
| Create | `src/NexusCore.EntityFrameworkCore/Saudi/Onboarding/EfCoreTenantOnboardingRepository.cs` |
| Modify | `src/NexusCore.EntityFrameworkCore/NexusCoreDbContext.cs` — add `DbSet<TenantOnboardingStatus>` |

**`TenantOnboardingStatus` entity:**
```csharp
public class TenantOnboardingStatus : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public bool ZatcaConfigured { get; set; }          // Step 1
    public bool SellerCreated { get; set; }             // Step 2
    public bool CertificateUploaded { get; set; }       // Step 3
    public bool FirstInvoiceSubmitted { get; set; }     // Step 4
    public bool NafathConfigured { get; set; }          // Step 5 (optional)
    public DateTime? CompletedAt { get; set; }
}
```

### 15B. Backend — Onboarding App Service

| Action | File |
|--------|------|
| Create | `src/NexusCore.Application.Contracts/Saudi/Onboarding/IOnboardingAppService.cs` |
| Create | `src/NexusCore.Application.Contracts/Saudi/Onboarding/Dtos/OnboardingStatusDto.cs` |
| Create | `src/NexusCore.Application/Saudi/Onboarding/OnboardingAppService.cs` |

**Methods:**
- `GetStatusAsync()` — returns current onboarding progress for the tenant
- `CompleteStepAsync(step)` — marks a step complete (called automatically when user performs the action)
- `ResetAsync()` — resets onboarding (admin only)

### 15C. Angular — Onboarding Wizard

| Action | File |
|--------|------|
| Create | `angular/src/app/saudi/onboarding/onboarding-wizard.component.ts` |
| Create | `angular/src/app/saudi/onboarding/services/onboarding.service.ts` |
| Modify | `angular/src/app/saudi/saudi.routes.ts` — add `/saudi/onboarding` route |

**Wizard UI (4-5 steps):**

1. **Welcome** — Overview of Saudi Kit capabilities, "Get Started" button
2. **Configure ZATCA** — Inline form for API URL, CSID, Secret. Environment selector. "Test Connection" button that pings ZATCA sandbox
3. **Create Seller** — Inline form for Arabic name, VAT, address. Pre-validates VAT format
4. **Upload Certificate** — File upload for PEM + private key. Displays certificate details after upload (issuer, expiry, subject)
5. **Submit First Invoice** — Quick-create simplified invoice form. One-click submit to ZATCA sandbox. Shows success/failure with ZATCA response

**Optional step 6: Configure Nafath** — only shown if tenant wants Nafath SSO

**Design:**
- Stepper/wizard UI with progress indicator
- Each step has "Skip" and "Next" buttons
- Completed steps show green checkmark
- Can revisit completed steps
- Progress persists across sessions (backed by `TenantOnboardingStatus`)
- On completion, redirects to Saudi dashboard

### Verification
- New tenant sees onboarding wizard on first visit to `/saudi`
- Each step saves progress to backend
- Completing all steps marks onboarding as complete
- Returning tenant sees dashboard, not wizard

---

## Phase 16: Dashboard & Analytics

**Priority: MEDIUM — gives admins visibility into operations**
**~10 files (8 new, 2 modified)**

### 16A. Backend — Dashboard App Service

| Action | File |
|--------|------|
| Create | `src/NexusCore.Application.Contracts/Saudi/Dashboard/ISaudiDashboardAppService.cs` |
| Create | `src/NexusCore.Application.Contracts/Saudi/Dashboard/Dtos/DashboardDto.cs` |
| Create | `src/NexusCore.Application/Saudi/Dashboard/SaudiDashboardAppService.cs` |

**`DashboardDto` structure:**
```csharp
public class DashboardDto
{
    // Invoice Statistics
    public int TotalInvoices { get; set; }
    public int DraftInvoices { get; set; }
    public int SubmittedInvoices { get; set; }     // Reported + Cleared
    public int RejectedInvoices { get; set; }
    public decimal TotalRevenue { get; set; }       // Sum of all cleared invoice totals
    public decimal TotalVat { get; set; }           // Sum of all cleared invoice VAT

    // Monthly Breakdown (last 12 months)
    public List<MonthlyInvoiceStatsDto> MonthlyStats { get; set; }

    // Nafath Statistics
    public int TotalNafathAuths { get; set; }
    public int CompletedNafathAuths { get; set; }
    public int ExpiredNafathAuths { get; set; }
    public int LinkedUsers { get; set; }

    // Approval Statistics
    public int PendingApprovals { get; set; }
    public int ApprovedThisMonth { get; set; }
    public int RejectedThisMonth { get; set; }

    // System Health
    public string ZatcaApiStatus { get; set; }      // Healthy/Degraded/Unhealthy
    public string NafathApiStatus { get; set; }
    public DateTime LastZatcaSubmission { get; set; }
}

public class MonthlyInvoiceStatsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthNameAr { get; set; }         // Hijri month name
    public int InvoiceCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal Vat { get; set; }
}
```

**Methods:**
- `GetDashboardAsync()` — aggregates all statistics
- `GetMonthlyStatsAsync(year)` — detailed monthly breakdown
- `GetRecentActivityAsync(count)` — last N invoice submissions/approvals

### 16B. Angular — Dashboard Components

| Action | File |
|--------|------|
| Create | `angular/src/app/saudi/dashboard/saudi-dashboard.component.ts` |
| Create | `angular/src/app/saudi/dashboard/components/invoice-stats-card.component.ts` |
| Create | `angular/src/app/saudi/dashboard/components/monthly-chart.component.ts` |
| Create | `angular/src/app/saudi/dashboard/components/recent-activity.component.ts` |
| Create | `angular/src/app/saudi/dashboard/services/dashboard.service.ts` |
| Modify | `angular/src/app/saudi/saudi.routes.ts` — add `/saudi/dashboard` as default route |

**Dashboard Layout:**
```
┌──────────────────────────────────────────────────────────┐
│ Saudi Kit Dashboard                            [Refresh] │
├──────────┬──────────┬──────────┬─────────────────────────┤
│ Total    │ Cleared  │ Rejected │ Revenue (SAR)           │
│ Invoices │ Invoices │ Invoices │ Total VAT               │
│   156    │   142    │    3     │ 1,234,567.89            │
│          │          │          │ 185,185.18              │
├──────────┴──────────┴──────────┴─────────────────────────┤
│ Monthly Invoice Volume (Bar Chart)                       │
│ ┌─────────────────────────────────────────────────────┐  │
│ │ █  █  ██ ██ ███ ███ ████ ████ █████ █████ ████ ███ │  │
│ │ J  F  M  A  M   J   J    A    S     O     N    D   │  │
│ └─────────────────────────────────────────────────────┘  │
├───────────────────────┬──────────────────────────────────┤
│ Nafath Stats          │ Approval Stats                   │
│ ● 89 Authentications  │ ● 12 Pending                     │
│ ● 82 Completed (92%)  │ ● 45 Approved this month         │
│ ● 34 Linked Users     │ ● 3 Rejected this month          │
├───────────────────────┴──────────────────────────────────┤
│ Recent Activity                                          │
│ ┌─ 10:32 Invoice #INV-2024-0156 cleared by ZATCA        │
│ ├─ 10:15 Purchase order #PO-789 approved by Ahmed        │
│ ├─ 09:45 Nafath auth completed for user Khalid           │
│ └─ 09:30 Invoice #INV-2024-0155 submitted for clearance  │
├──────────────────────────────────────────────────────────┤
│ System Health                                            │
│ ● ZATCA API: Healthy  ● Nafath API: Healthy              │
│ Last submission: 2 minutes ago                           │
└──────────────────────────────────────────────────────────┘
```

**Chart library:** Use native SVG or lightweight chart (no heavy dependencies like Chart.js). Alternatively, `ngx-charts` if already in ABP's dependency tree.

### Verification
- Dashboard loads with real data from API
- Stats cards show correct counts
- Monthly chart renders with actual monthly data
- Recent activity shows last 10 events
- Health indicators reflect real health check status
- Dashboard is the default page when navigating to `/saudi`

---

## Phase 17: Audit Log Viewer

**Priority: MEDIUM — leverages ABP's built-in audit logging**
**~6 files (5 new, 1 modified)**

### 17A. Backend — Saudi Audit Service

ABP already logs all authenticated API calls. We need a thin wrapper to filter Saudi-specific entries.

| Action | File |
|--------|------|
| Create | `src/NexusCore.Application.Contracts/Saudi/Audit/ISaudiAuditAppService.cs` |
| Create | `src/NexusCore.Application.Contracts/Saudi/Audit/Dtos/SaudiAuditLogDto.cs` |
| Create | `src/NexusCore.Application/Saudi/Audit/SaudiAuditAppService.cs` |

**Methods:**
- `GetListAsync(input)` — paginated list of audit entries filtered to Saudi module
  - Filter by: date range, user, action type (ZATCA/Nafath/Workflow), entity type, entity ID
  - Includes: timestamp, user, action, entity, HTTP method, URL, duration, status code
- `GetEntityHistoryAsync(entityType, entityId)` — change history for a specific entity (e.g., an invoice's lifecycle)

**Filters** (via `GetSaudiAuditListInput`):
```csharp
public class GetSaudiAuditListInput : PagedAndSortedResultRequestDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? UserId { get; set; }
    public string? Module { get; set; }          // "Zatca", "Nafath", "Workflow"
    public string? EntityType { get; set; }       // "ZatcaInvoice", "ZatcaSeller", etc.
    public string? EntityId { get; set; }
    public int? MinDuration { get; set; }         // Slow request filter (ms)
}
```

### 17B. Angular — Audit Log UI

| Action | File |
|--------|------|
| Create | `angular/src/app/saudi/audit/saudi-audit-log.component.ts` |
| Create | `angular/src/app/saudi/audit/services/audit.service.ts` |
| Modify | `angular/src/app/saudi/saudi.routes.ts` — add `/saudi/audit` route |

**UI Layout:**
- **Filter bar**: Date range picker, user dropdown, module dropdown (All/ZATCA/Nafath/Workflows), search by entity ID
- **Results table**: Timestamp, User, Action, Entity, Duration (ms), Status (color-coded: green ≤200ms, yellow ≤1s, red >1s)
- **Detail panel**: Click a row to expand and see full request/response details, changed properties
- **Export**: "Export CSV" button for compliance/audit purposes
- **Pagination**: Server-side, 25 rows per page

**Entity History Sub-view:**
- Accessible from invoice detail / seller detail via "View History" button
- Shows timeline of all changes to that entity
- Each entry shows: who changed it, when, what fields changed (before → after)

### Verification
- Audit log page shows real ABP audit entries filtered to Saudi module
- Filters narrow results correctly
- Entity history shows complete change timeline
- CSV export downloads valid file
- Performance: page loads in under 2 seconds for 10,000+ audit entries

---

## Phase 18: End-to-End Tests (Playwright)

**Priority: MEDIUM — critical user flows need E2E coverage**
**~12 files (all new)**

### 18A. Playwright Setup

| Action | File |
|--------|------|
| Create | `e2e/playwright.config.ts` |
| Create | `e2e/package.json` |
| Create | `e2e/tsconfig.json` |
| Create | `e2e/fixtures/auth.fixture.ts` |

**Configuration:**
- Base URL: `http://localhost:4200`
- Browsers: Chromium (primary), Firefox (secondary)
- Parallel execution with 2 workers
- Screenshots on failure
- HTML reporter
- Auth fixture: logs in once, saves session state, reuses across tests

### 18B. E2E Test Suites (7 files)

| File | Flow | Steps |
|------|------|-------|
| `e2e/tests/zatca-invoice-lifecycle.spec.ts` | Complete invoice lifecycle | Navigate to invoices → Create new invoice → Fill seller, buyer, lines → Save as draft → Validate → Submit to ZATCA → Verify status changes → View QR code → Download PDF |
| `e2e/tests/zatca-seller-management.spec.ts` | Seller CRUD | Navigate to sellers → Create seller with Arabic name → Verify in list → Edit VAT number → Set as default → Delete non-default seller |
| `e2e/tests/zatca-certificate-management.spec.ts` | Certificate CRUD | Navigate to settings → Select seller → Upload certificate → Activate → Verify active badge → Deactivate |
| `e2e/tests/nafath-login-flow.spec.ts` | Nafath auth (mocked API) | Navigate to Nafath login → Enter National ID → Verify random number displayed → Mock callback → Verify success state |
| `e2e/tests/nafath-link-identity.spec.ts` | Identity linking | Navigate to link page → Verify unlinked state → Initiate link → Mock completion → Verify linked state → Unlink → Verify unlinked |
| `e2e/tests/approval-workflow.spec.ts` | Approval flow | Navigate to inbox → View pending task → Add comment → Approve → Verify status → Navigate to inbox → View another task → Reject |
| `e2e/tests/settings-management.spec.ts` | Settings CRUD | Navigate to settings → Update ZATCA settings → Save → Refresh → Verify persisted → Switch to Nafath tab → Update → Save → Verify |

### 18C. CI Integration

| Action | File |
|--------|------|
| Create | `e2e/Dockerfile` — Playwright container with browsers |
| Modify | `.github/workflows/ci.yml` — add E2E job (depends on backend + frontend build), runs against Docker Compose |

**E2E job:**
```yaml
e2e:
  needs: [backend, frontend]
  runs-on: ubuntu-latest
  steps:
    - docker compose up -d
    - npx wait-on http://localhost:4200
    - cd e2e && npx playwright test
    - upload test-results as artifact
```

### Verification
- `cd e2e && npx playwright test` — all 7 suites pass
- CI runs E2E on every PR
- Failure screenshots captured and uploaded as artifacts

---

## Phase 19: Performance & Caching

**Priority: MEDIUM-LOW — optimization after correctness**
**~8 files (5 new, 3 modified)**

### 19A. Backend — Response Caching

| Action | File |
|--------|------|
| Create | `src/NexusCore.Application/Saudi/Caching/SaudiCacheKeys.cs` |

**Cache keys:**
```csharp
public static class SaudiCacheKeys
{
    public static string HijriToday() => "saudi:hijri:today";
    public static string Sellers(Guid tenantId) => $"saudi:sellers:{tenantId}";
    public static string DefaultSeller(Guid tenantId) => $"saudi:sellers:{tenantId}:default";
    public static string DashboardStats(Guid tenantId) => $"saudi:dashboard:{tenantId}";
    public static string InvoiceCount(Guid tenantId) => $"saudi:invoices:{tenantId}:count";
}
```

**Caching strategy:**
- `HijriToday`: cached for 24 hours (changes daily)
- `Sellers`: cached for 10 minutes, invalidated on create/update/delete
- `DefaultSeller`: cached for 10 minutes, invalidated on `SetAsDefault`
- `DashboardStats`: cached for 5 minutes
- `InvoiceCount`: cached for 1 minute

### 19B. Backend — Query Optimization

| Action | File |
|--------|------|
| Modify | `src/NexusCore.EntityFrameworkCore/NexusCoreDbContext.cs` — add missing indexes |

**Additional indexes:**
```csharp
// ZatcaInvoice — common query patterns
builder.Entity<ZatcaInvoice>(b =>
{
    b.HasIndex(x => x.Status);
    b.HasIndex(x => x.IssueDate);
    b.HasIndex(x => new { x.SellerId, x.Status });
    b.HasIndex(x => new { x.TenantId, x.Status, x.IssueDate }); // Dashboard queries
});

// ApprovalTask — inbox queries
builder.Entity<ApprovalTask>(b =>
{
    b.HasIndex(x => new { x.AssignedToUserId, x.Status });
    b.HasIndex(x => new { x.EntityType, x.EntityId });
});

// NafathAuthRequest — status check
builder.Entity<NafathAuthRequest>(b =>
{
    b.HasIndex(x => x.TransactionId).IsUnique();
});
```

### 19C. Angular — Lazy Loading & Bundle Optimization

| Action | File |
|--------|------|
| Modify | `angular/src/app/saudi/saudi.routes.ts` — verify all routes use `loadComponent` (already done) |
| Create | `angular/src/app/saudi/shared/services/cache.service.ts` |

**`cache.service.ts`:**
- Simple in-memory cache with TTL
- Used by services to cache frequently-accessed data (sellers list, Hijri today, dashboard stats)
- `get<T>(key, fetcher, ttlMs)` — returns cached value or calls fetcher
- `invalidate(key)` — clears specific key
- `clear()` — clears all cached data

### 19D. Database Migration for New Indexes

| Action | File |
|--------|------|
| Create | EF Core migration via `dotnet ef migrations add AddPerformanceIndexes` |

### Verification
- Invoice list page loads in <500ms for 1000+ invoices
- Dashboard loads in <1s with cached data
- Seller dropdown loads instantly after first fetch
- Bundle size: Saudi module chunk <200KB gzipped
- Database queries use indexes (verify with `SET STATISTICS IO ON` or query plans)

---

## Phase 20: Release v1.0.0 & Changelog

**Priority: FINAL — ship it**
**~5 files (all new)**

### 20A. Changelog & Release Notes

| Action | File |
|--------|------|
| Create | `CHANGELOG.md` |

**Format:** [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)

```markdown
# Changelog

## [1.0.0] - 2025-XX-XX

### Added
- ZATCA E-Invoicing: full lifecycle (draft → validate → submit → cleared/reported)
- ZATCA invoice types: Standard (B2B), Simplified (B2C), Credit Note, Debit Note
- ZATCA QR code generation (TLV Base64, scannable)
- ZATCA PDF export with Arabic support
- ZATCA XML builder (UBL 2.1 compliant)
- ZATCA digital signature (X.509 SHA-256)
- ZATCA webhook for async responses (HMAC-SHA256 validated)
- ZATCA seller management with default seller
- ZATCA certificate management per seller
- Nafath SSO integration (initiate, poll, callback)
- Nafath identity linking/unlinking per user
- Approval workflow engine with task assignment
- Approval delegation with date-based expiry
- Hijri calendar with Gregorian ↔ Hijri conversion
- Dual calendar picker component
- Saudi settings management (ZATCA + Nafath per tenant)
- Multi-tenant onboarding wizard
- Dashboard with invoice/Nafath/workflow statistics
- Audit log viewer filtered to Saudi module
- Arabic localization (ar.json) with full RTL support
- Docker Compose deployment (API + Angular + SQL Server)
- GitHub Actions CI/CD (build, test, release)
- Health checks for ZATCA and Nafath APIs
- Rate limiting on callback/webhook endpoints
- Polly resilience (retry, circuit breaker, timeout)
- OpenTelemetry metrics and tracing
- Demo data seeder for new tenants
- 129+ backend tests (domain, application, EF Core, integration)
- Angular component tests (80%+ coverage)
- Playwright E2E tests (7 critical flows)
- Comprehensive documentation (deployment, operations, security, ADRs)
```

### 20B. Version Bumping

| Action | File |
|--------|------|
| Modify | All `.csproj` files — add `<Version>1.0.0</Version>` |
| Modify | `angular/package.json` — set `"version": "1.0.0"` |

### 20C. Git Tag & GitHub Release

```bash
git tag -a v1.0.0 -m "Release v1.0.0 — Saudi Kit production-ready"
git push origin v1.0.0
```

This triggers `.github/workflows/release.yml`:
- Builds Docker images
- Pushes to GitHub Container Registry (GHCR)
- Creates GitHub Release with changelog

### 20D. License

| Action | File |
|--------|------|
| Create | `LICENSE` — choose appropriate license (MIT for open-source, or proprietary) |

### Verification
- `v1.0.0` tag exists on `main`
- GitHub Release page shows full changelog
- Docker images available at `ghcr.io/alsairy/nexuscore-api:v1.0.0` and `ghcr.io/alsairy/nexuscore-angular:v1.0.0`
- `CHANGELOG.md` is complete and accurate

---

## Dependency Graph

```
Phase 9  (README & Docs)           ── independent, do first
Phase 10 (Angular Tests)           ── independent, parallelizable
Phase 11 (App Service Tests)       ── independent, parallelizable
Phase 12 (Arabic & RTL)            ── independent, parallelizable

Phase 13 (ZATCA Sandbox)           ── depends on Phase 11
Phase 14 (Nafath Sandbox)          ── depends on Phase 13 (shared test project)

Phase 15 (Onboarding Wizard)       ── depends on Phase 12 (needs localization)
Phase 16 (Dashboard)               ── depends on Phase 12 (needs localization)
Phase 17 (Audit Log)               ── depends on Phase 12 (needs localization)

Phase 18 (E2E Tests)               ── depends on Phases 15, 16, 17 (tests those features)
Phase 19 (Performance)             ── depends on Phase 16 (caches dashboard queries)

Phase 20 (Release v1.0.0)          ── depends on ALL previous phases
```

```
 9 ──────────────────────────────────────────────────────────────┐
10 ──────────────────────────────────────────────────────────────┤
11 ────────────┬─────────────────────────────────────────────────┤
12 ────────────┼──────────────┬──────────┬──────────┐            │
               │              │          │          │            │
              13 ─────┬──    15         16         17            │
                      │                  │                       │
                     14                  │                       │
                                         │                       │
                      ├──────────────── 18                       │
                      │                  │                       │
                      │                 19                       │
                      │                  │                       │
                      └──────────────── 20 ◄─────────────────────┘
```

### Parallelism Opportunities

**Batch 1 (fully parallel):**
- Phase 9: README & Docs
- Phase 10: Angular Tests
- Phase 11: App Service Tests
- Phase 12: Arabic & RTL

**Batch 2 (after Batch 1):**
- Phase 13: ZATCA Sandbox (needs 11)
- Phase 15: Onboarding Wizard (needs 12)
- Phase 16: Dashboard (needs 12)
- Phase 17: Audit Log (needs 12)

**Batch 3 (after Batch 2):**
- Phase 14: Nafath Sandbox (needs 13)
- Phase 18: E2E Tests (needs 15, 16, 17)
- Phase 19: Performance (needs 16)

**Batch 4 (final):**
- Phase 20: Release v1.0.0 (needs all)

---

## Totals

| Phase | New Files | Modified Files | Total Files | Key Deliverables |
|-------|-----------|----------------|-------------|------------------|
| 9. README & Docs | 3 | 1 | 4 | Professional README, ADRs |
| 10. Angular Tests | 18 | 0 | 18 | 14 component tests, 2 service tests, 2 helpers |
| 11. App Service Tests | 7 | 0 | 7 | 40+ application test cases |
| 12. Arabic & RTL | 4 | 10 | 14 | Full Arabic translation, RTL layout |
| 13. ZATCA Sandbox | 4 | 2 | 6 | Integration test project, XML compliance |
| 14. Nafath Sandbox | 3 | 0 | 3 | Nafath + webhook security tests |
| 15. Onboarding Wizard | 10 | 2 | 12 | 5-step guided setup for new tenants |
| 16. Dashboard | 8 | 2 | 10 | Stats cards, charts, recent activity |
| 17. Audit Log | 5 | 1 | 6 | Filtered audit viewer, entity history, CSV export |
| 18. E2E Tests | 12 | 0 | 12 | Playwright setup, 7 critical flow suites |
| 19. Performance | 5 | 3 | 8 | Response caching, query indexes, bundle optimization |
| 20. Release v1.0.0 | 2 | ~16 | ~18 | Changelog, version bump, git tag, GitHub Release |
| **TOTAL** | **~81** | **~37** | **~118** | |

---

## Key Reference Files

| Purpose | Path |
|---------|------|
| Existing README to rewrite | `README.md` |
| Angular components (all Saudi) | `angular/src/app/saudi/` |
| Backend app services | `src/NexusCore.Application/Saudi/` |
| Domain entities | `src/NexusCore.Domain/Saudi/` |
| EF Core config | `src/NexusCore.EntityFrameworkCore/NexusCoreDbContext.cs` |
| Existing test base | `test/NexusCore.TestBase/` |
| Existing domain tests | `test/NexusCore.Domain.Tests/Saudi/` |
| Existing EF Core tests | `test/NexusCore.EntityFrameworkCore.Tests/Saudi/` |
| Localization resources | `src/NexusCore.Domain.Shared/Localization/NexusCore/` |
| Saudi routes | `angular/src/app/saudi/saudi.routes.ts` |
| CI/CD workflows | `.github/workflows/` |
| Deployment guide | `docs/deployment-guide.md` |
| Security review | `docs/security-review.md` |
| Operations guide | `docs/saudi-kit-operations.md` |
| Docker Compose | `docker-compose.yml` |
