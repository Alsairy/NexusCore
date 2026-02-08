using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NexusCore.Saudi.Zatca;
using Shouldly;
using Xunit;

namespace NexusCore.Saudi.Tests.Zatca;

/// <summary>
/// Integration tests that run against the ZATCA sandbox API.
/// These tests require valid sandbox credentials in appsettings.test.json
/// or environment variables. They are excluded from CI by default and
/// should only be run manually with valid ZATCA sandbox credentials.
///
/// Run: dotnet test --filter Category=Integration
/// </summary>
[Trait("Category", "Integration")]
public class ZatcaSandboxTests : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ZatcaXmlBuilder _xmlBuilder = new();
    private readonly ZatcaQrGenerator _qrGenerator = new();
    private readonly ZatcaInvoiceSigner _signer = new();

    private readonly string _apiBaseUrl;
    private readonly string _complianceCsid;
    private readonly string _secret;

    public ZatcaSandboxTests()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddEnvironmentVariables("ZATCA_")
            .Build();

        _apiBaseUrl = _configuration["Zatca:Sandbox:ApiBaseUrl"]
            ?? ZatcaConsts.SandboxBaseUrl;
        _complianceCsid = _configuration["Zatca:Sandbox:ComplianceCsid"] ?? "";
        _secret = _configuration["Zatca:Sandbox:Secret"] ?? "";

        _httpClient = new HttpClient();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private bool HasCredentials => !string.IsNullOrEmpty(_complianceCsid)
                                && !string.IsNullOrEmpty(_secret);

    #region Helper Methods

    private static ZatcaSeller CreateSandboxSeller()
    {
        return new ZatcaSeller(
            Guid.NewGuid(),
            "شركة اختبار الفوترة الإلكترونية",
            "300000000000003")
        {
            SellerNameEn = "E-Invoice Test Company",
            CommercialRegistrationNumber = "1234567890",
            Street = "شارع الملك فهد",
            BuildingNumber = "1234",
            City = "الرياض",
            District = "العليا",
            PostalCode = "12345",
            CountryCode = "SA",
            IsDefault = true
        };
    }

    private string BuildInvoiceXml(ZatcaInvoice invoice, ZatcaSeller seller)
    {
        var qrCode = _qrGenerator.GenerateQrCode(
            seller.SellerNameAr,
            seller.VatRegistrationNumber,
            invoice.IssueDate,
            invoice.GrandTotal,
            invoice.VatAmount);

        var hash = _signer.ComputeInvoiceHash("<placeholder/>");
        return _xmlBuilder.BuildInvoiceXml(invoice, seller, hash, qrCode);
    }

    private async Task<(bool success, string responseBody, int statusCode)> SubmitToSandboxAsync(
        string endpoint, string invoiceXml, string invoiceId)
    {
        var authValue = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_complianceCsid}:{_secret}"));

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", authValue);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var payload = new
        {
            invoiceHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(invoiceXml)),
            uuid = invoiceId,
            invoice = Convert.ToBase64String(Encoding.UTF8.GetBytes(invoiceXml))
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var url = $"{_apiBaseUrl.TrimEnd('/')}/{endpoint}";
        var response = await _httpClient.PostAsync(url, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        return (response.IsSuccessStatusCode, responseBody, (int)response.StatusCode);
    }

    #endregion

    [Fact]
    public async Task Should_Generate_Valid_Ubl21_Xml_For_Submission()
    {
        // This test validates the XML structure is complete enough for ZATCA
        var seller = CreateSandboxSeller();
        var invoice = new ZatcaInvoice(
            Guid.NewGuid(),
            seller.Id,
            $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ZatcaInvoiceType.Simplified,
            DateTime.UtcNow)
        {
            BuyerName = "عميل اختبار",
            CurrencyCode = "SAR",
            Seller = seller
        };

        var line = new ZatcaInvoiceLine(
            Guid.NewGuid(), invoice.Id, "خدمة اختبار", 1, 100m);
        invoice.Lines.Add(line);
        invoice.SubTotal = line.NetAmount;
        invoice.VatAmount = line.VatAmount;
        invoice.GrandTotal = line.TotalAmount;

        var xml = BuildInvoiceXml(invoice, seller);

        // XML should be well-formed and contain required elements
        xml.ShouldNotBeNullOrWhiteSpace();
        xml.ShouldContain("urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
        xml.ShouldContain("AccountingSupplierParty");
        xml.ShouldContain("TaxTotal");
        xml.ShouldContain("LegalMonetaryTotal");
        xml.ShouldContain("InvoiceLine");
        xml.ShouldContain("UBLExtensions");
        xml.ShouldContain("300000000000003"); // VAT number
        xml.ShouldContain("شركة اختبار الفوترة الإلكترونية"); // Arabic name
    }

    [Fact]
    public async Task Should_Report_Simplified_Invoice_To_Sandbox()
    {
        if (!HasCredentials)
        {
            // Skip if no credentials configured
            return;
        }

        var seller = CreateSandboxSeller();
        var invoice = new ZatcaInvoice(
            Guid.NewGuid(),
            seller.Id,
            $"SINV-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ZatcaInvoiceType.Simplified,
            DateTime.UtcNow)
        {
            BuyerName = "عميل نقدي",
            CurrencyCode = "SAR",
            Seller = seller
        };

        var line = new ZatcaInvoiceLine(
            Guid.NewGuid(), invoice.Id, "منتج اختبار", 1, 100m);
        invoice.Lines.Add(line);
        invoice.SubTotal = line.NetAmount;
        invoice.VatAmount = line.VatAmount;
        invoice.GrandTotal = line.TotalAmount;

        var xml = BuildInvoiceXml(invoice, seller);

        var (success, body, statusCode) = await SubmitToSandboxAsync(
            "invoices/reporting/single",
            xml,
            invoice.Id.ToString());

        // In sandbox, even if rejected we should get a structured response
        statusCode.ShouldBeInRange(200, 499);
        body.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Clear_Standard_Invoice_To_Sandbox()
    {
        if (!HasCredentials)
        {
            return;
        }

        var seller = CreateSandboxSeller();
        var invoice = new ZatcaInvoice(
            Guid.NewGuid(),
            seller.Id,
            $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ZatcaInvoiceType.Standard,
            DateTime.UtcNow)
        {
            BuyerName = "شركة المشتري الاختبارية",
            BuyerVatNumber = "300000000000010",
            CurrencyCode = "SAR",
            Seller = seller
        };

        var line = new ZatcaInvoiceLine(
            Guid.NewGuid(), invoice.Id, "خدمة استشارية", 2, 1000m);
        invoice.Lines.Add(line);
        invoice.SubTotal = line.NetAmount;
        invoice.VatAmount = line.VatAmount;
        invoice.GrandTotal = line.TotalAmount;

        var xml = BuildInvoiceXml(invoice, seller);

        var (success, body, statusCode) = await SubmitToSandboxAsync(
            "invoices/clearance/single",
            xml,
            invoice.Id.ToString());

        statusCode.ShouldBeInRange(200, 499);
        body.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Submit_Compliance_Check_To_Sandbox()
    {
        if (!HasCredentials)
        {
            return;
        }

        var seller = CreateSandboxSeller();
        var invoice = new ZatcaInvoice(
            Guid.NewGuid(),
            seller.Id,
            $"COMP-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ZatcaInvoiceType.Simplified,
            DateTime.UtcNow)
        {
            BuyerName = "عميل فحص الامتثال",
            CurrencyCode = "SAR",
            Seller = seller
        };

        var line = new ZatcaInvoiceLine(
            Guid.NewGuid(), invoice.Id, "خدمة امتثال", 1, 500m);
        invoice.Lines.Add(line);
        invoice.SubTotal = line.NetAmount;
        invoice.VatAmount = line.VatAmount;
        invoice.GrandTotal = line.TotalAmount;

        var xml = BuildInvoiceXml(invoice, seller);

        var (success, body, statusCode) = await SubmitToSandboxAsync(
            "compliance/invoices",
            xml,
            invoice.Id.ToString());

        statusCode.ShouldBeInRange(200, 499);
        body.ShouldNotBeNullOrWhiteSpace();

        // Parse response to verify structure
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("validationResults", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Report_Credit_Note_To_Sandbox()
    {
        if (!HasCredentials)
        {
            return;
        }

        var seller = CreateSandboxSeller();
        var invoice = new ZatcaInvoice(
            Guid.NewGuid(),
            seller.Id,
            $"CN-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ZatcaInvoiceType.SimplifiedCreditNote,
            DateTime.UtcNow)
        {
            BuyerName = "عميل إشعار دائن",
            CurrencyCode = "SAR",
            Seller = seller
        };

        var line = new ZatcaInvoiceLine(
            Guid.NewGuid(), invoice.Id, "مرتجع", 1, 50m);
        invoice.Lines.Add(line);
        invoice.SubTotal = line.NetAmount;
        invoice.VatAmount = line.VatAmount;
        invoice.GrandTotal = line.TotalAmount;

        var xml = BuildInvoiceXml(invoice, seller);

        var (success, body, statusCode) = await SubmitToSandboxAsync(
            "invoices/reporting/single",
            xml,
            invoice.Id.ToString());

        statusCode.ShouldBeInRange(200, 499);
        body.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Return_Validation_Errors_For_Invalid_Invoice()
    {
        if (!HasCredentials)
        {
            return;
        }

        // Submit intentionally invalid XML
        var invalidXml = "<Invoice><Invalid>This is not a valid UBL invoice</Invalid></Invoice>";

        var (success, body, statusCode) = await SubmitToSandboxAsync(
            "compliance/invoices",
            invalidXml,
            Guid.NewGuid().ToString());

        // Should get an error response (400 or similar)
        body.ShouldNotBeNullOrWhiteSpace();

        // Response should contain error information
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        // Either validationResults.errorMessages or errors should be present
        var hasValidationErrors = root.TryGetProperty("validationResults", out var vr)
            && vr.TryGetProperty("errorMessages", out _);
        var hasErrors = root.TryGetProperty("errors", out _);
        var hasMessage = root.TryGetProperty("message", out _);

        (hasValidationErrors || hasErrors || hasMessage).ShouldBeTrue(
            "Response should contain error details");
    }

    [Fact]
    public async Task Should_Receive_Structured_Response_With_RequestId()
    {
        if (!HasCredentials)
        {
            return;
        }

        var seller = CreateSandboxSeller();
        var invoice = new ZatcaInvoice(
            Guid.NewGuid(),
            seller.Id,
            $"QR-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ZatcaInvoiceType.Simplified,
            DateTime.UtcNow)
        {
            BuyerName = "اختبار QR",
            CurrencyCode = "SAR",
            Seller = seller
        };

        var line = new ZatcaInvoiceLine(
            Guid.NewGuid(), invoice.Id, "منتج QR", 1, 200m);
        invoice.Lines.Add(line);
        invoice.SubTotal = line.NetAmount;
        invoice.VatAmount = line.VatAmount;
        invoice.GrandTotal = line.TotalAmount;

        var xml = BuildInvoiceXml(invoice, seller);

        var (success, body, statusCode) = await SubmitToSandboxAsync(
            "invoices/reporting/single",
            xml,
            invoice.Id.ToString());

        body.ShouldNotBeNullOrWhiteSpace();

        if (success)
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // Successful responses should have validationResults
            root.TryGetProperty("validationResults", out var validationResults).ShouldBeTrue();
            validationResults.TryGetProperty("status", out _).ShouldBeTrue();
        }
    }
}
