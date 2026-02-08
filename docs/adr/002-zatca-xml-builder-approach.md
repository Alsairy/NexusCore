# ADR-002: ZATCA XML Builder Approach

## Status

Accepted

## Date

2024-12-15

## Context

ZATCA (Zakat, Tax and Customs Authority) requires all e-invoices to be submitted as UBL 2.1 XML documents. The XML must:

- Conform to the OASIS UBL 2.1 standard with ZATCA-specific extensions
- Include 10+ XML namespaces (`cac`, `cbc`, `ext`, `sig`, `sac`, `ds`, `xades`)
- Contain digital signatures (XMLDSIG with X.509 certificates)
- Be hash-signed with SHA-256 for integrity
- Include a TLV-encoded QR code with 5 mandatory tags
- Support both Standard (B2B clearance) and Simplified (B2C reporting) profiles

Approaches evaluated:

1. **String concatenation / template literals** — Simple but fragile. No namespace validation, easy to produce malformed XML, difficult to maintain as ZATCA specs evolve.
2. **XDocument / XElement (System.Xml.Linq)** — .NET built-in XML API. Handles namespaces correctly, produces well-formed XML, supports XPath queries for signature insertion. No external dependencies.
3. **Third-party UBL library (UblSharp, UBL.NET)** — Strongly-typed UBL models. However, ZATCA uses custom extensions (`sac:` namespace) not covered by standard UBL libraries, requiring workarounds.
4. **XSLT transformation** — Define invoice data as simple XML, transform to UBL via XSLT stylesheet. Good for separation of concerns but adds complexity and makes debugging harder.

## Decision

We chose **`XDocument` / `XElement` (System.Xml.Linq)** for XML generation, implemented in `ZatcaXmlBuilder` as a domain service.

Design:

```
ZatcaXmlBuilder (Domain Service)
├── BuildInvoiceXml(invoice, seller, certificate)
│   ├── CreateRootElement()          — UBL Invoice with all namespace declarations
│   ├── AddExtensions()              — UBL extensions block (signature placeholder)
│   ├── AddProfileId()               — "reporting:1.0" or "clearance:1.0"
│   ├── AddSupplierParty()           — Seller info with Arabic name, VAT, address
│   ├── AddCustomerParty()           — Buyer info (required for B2B, optional for B2C)
│   ├── AddInvoiceLines()            — Line items with quantities, prices, tax categories
│   ├── AddTaxTotal()                — Calculated tax totals matching line items
│   └── AddLegalMonetaryTotal()      — Subtotal, tax, total amounts
│
ZatcaInvoiceSigner (Domain Service)
├── ComputeInvoiceHash(xml)          — SHA-256 hash of canonicalized XML
└── SignInvoice(xml, certificate)     — XMLDSIG with X.509 certificate
│
ZatcaQrGenerator (Domain Service)
└── GenerateQrCode(invoice, seller)  — TLV-encoded Base64 with 5 tags
```

Key implementation choices:

- **Namespace constants** defined once and reused across all element creation methods
- **Separate domain services** for XML building, signing, and QR generation (single responsibility)
- **No external XML dependencies** — `System.Xml.Linq` is part of the .NET runtime
- **Validation before generation** — `ZatcaInvoiceValidator` runs before `ZatcaXmlBuilder` to catch business rule violations early

## Consequences

**Positive:**
- Zero external dependencies for XML generation
- Full control over ZATCA-specific extensions (`sac:` namespace) without fighting a UBL library's object model
- Easy to unit test — pass an invoice entity, assert XPath expressions on the output
- Namespace handling is correct by construction (XNamespace in System.Xml.Linq)
- Easy to update when ZATCA releases new specification versions

**Negative:**
- No compile-time validation of UBL structure — must rely on integration tests against ZATCA sandbox to catch structural errors
- Manual namespace management (must remember to declare all 7 namespaces on root element)
- XML builder methods are verbose (each UBL element is constructed manually)
- If ZATCA ever requires XSD validation, we'd need to add an XSD validation step separately
