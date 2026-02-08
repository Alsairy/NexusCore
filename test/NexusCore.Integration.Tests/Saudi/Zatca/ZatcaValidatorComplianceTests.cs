using System;
using System.Linq;
using NexusCore.Saudi.Zatca;
using Shouldly;
using Xunit;

namespace NexusCore.Saudi.Tests.Zatca;

/// <summary>
/// Tests that validate the ZatcaInvoiceValidator enforces all
/// ZATCA compliance rules for both standard and simplified invoices.
/// </summary>
public class ZatcaValidatorComplianceTests
{
    private readonly ZatcaInvoiceValidator _validator = new();

    #region Helper Methods

    private static ZatcaSeller CreateValidSeller()
    {
        return new ZatcaSeller(
            Guid.NewGuid(),
            "شركة الاختبار",
            "300000000000003")
        {
            CommercialRegistrationNumber = "1234567890",
            Street = "شارع الملك فهد",
            BuildingNumber = "1234",
            City = "الرياض",
            District = "العليا",
            PostalCode = "12345",
            CountryCode = "SA"
        };
    }

    private static ZatcaInvoice CreateValidStandardInvoice()
    {
        var seller = CreateValidSeller();
        var invoice = new ZatcaInvoice(
            Guid.NewGuid(),
            seller.Id,
            "INV-001",
            ZatcaInvoiceType.Standard,
            DateTime.UtcNow.AddDays(-1))
        {
            BuyerName = "شركة المشتري",
            BuyerVatNumber = "300000000000010",
            CurrencyCode = "SAR",
            Seller = seller
        };

        var line = new ZatcaInvoiceLine(
            Guid.NewGuid(), invoice.Id, "خدمة", 1, 1000m);
        invoice.Lines.Add(line);

        invoice.SubTotal = line.NetAmount;
        invoice.VatAmount = line.VatAmount;
        invoice.GrandTotal = line.TotalAmount;

        return invoice;
    }

    private static ZatcaInvoice CreateValidSimplifiedInvoice()
    {
        var seller = CreateValidSeller();
        var invoice = new ZatcaInvoice(
            Guid.NewGuid(),
            seller.Id,
            "SINV-001",
            ZatcaInvoiceType.Simplified,
            DateTime.UtcNow.AddDays(-1))
        {
            CurrencyCode = "SAR",
            Seller = seller
        };

        var line = new ZatcaInvoiceLine(
            Guid.NewGuid(), invoice.Id, "منتج", 2, 50m);
        invoice.Lines.Add(line);

        invoice.SubTotal = line.NetAmount;
        invoice.VatAmount = line.VatAmount;
        invoice.GrandTotal = line.TotalAmount;

        return invoice;
    }

    #endregion

    #region Valid Invoice Tests

    [Fact]
    public void Valid_Standard_Invoice_Should_Pass_Validation()
    {
        var invoice = CreateValidStandardInvoice();
        var errors = _validator.Validate(invoice);
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void Valid_Simplified_Invoice_Should_Pass_Validation()
    {
        var invoice = CreateValidSimplifiedInvoice();
        var errors = _validator.Validate(invoice);

        // Filter out the "Warning: Currency code is SAR" if present
        var realErrors = errors.Where(e => !e.StartsWith("Warning:")).ToList();
        realErrors.ShouldBeEmpty();
    }

    [Fact]
    public void IsValid_Should_Return_True_For_Valid_Invoice()
    {
        var invoice = CreateValidStandardInvoice();
        _validator.IsValid(invoice).ShouldBeTrue();
    }

    #endregion

    #region Missing Fields Tests

    [Fact]
    public void Invoice_Without_Number_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        invoice.InvoiceNumber = "";

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("Invoice number is required"));
    }

    [Fact]
    public void Invoice_Without_Seller_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        invoice.Seller = null!;

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("Seller information is required"));
    }

    [Fact]
    public void Invoice_Without_Lines_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        invoice.Lines.Clear();

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("at least one line item"));
    }

    #endregion

    #region VAT Number Validation Tests

    [Fact]
    public void Invalid_Seller_VAT_Number_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        invoice.Seller!.VatRegistrationNumber = "1234567890"; // Not 15 digits starting with 3

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e =>
            e.Contains("Seller VAT") && e.Contains("15 digits starting with 3"));
    }

    [Fact]
    public void Empty_Seller_VAT_Number_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        invoice.Seller!.VatRegistrationNumber = "";

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("Seller VAT registration number is required"));
    }

    [Fact]
    public void Invalid_Buyer_VAT_For_Standard_Invoice_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        invoice.BuyerVatNumber = "INVALID";

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e =>
            e.Contains("Buyer VAT") && e.Contains("15 digits starting with 3"));
    }

    #endregion

    #region B2B (Standard) Invoice Tests

    [Fact]
    public void Standard_Invoice_Without_Buyer_Name_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        invoice.BuyerName = null;

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("Buyer name is required for standard"));
    }

    [Fact]
    public void Standard_Invoice_Without_Buyer_VAT_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        invoice.BuyerVatNumber = null;

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("Buyer VAT number is required for standard"));
    }

    [Fact]
    public void Standard_Invoice_Without_Seller_Address_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        invoice.Seller!.Street = null;
        invoice.Seller!.City = null;

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("Seller street address is required"));
        errors.ShouldContain(e => e.Contains("Seller city is required"));
    }

    [Fact]
    public void Simplified_Invoice_Without_Buyer_Should_Pass()
    {
        var invoice = CreateValidSimplifiedInvoice();
        invoice.BuyerName = null;
        invoice.BuyerVatNumber = null;

        var errors = _validator.Validate(invoice);
        var realErrors = errors.Where(e => !e.StartsWith("Warning:")).ToList();
        realErrors.ShouldBeEmpty();
    }

    #endregion

    #region Invoice Line Validation Tests

    [Fact]
    public void Line_With_Negative_Quantity_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        var line = invoice.Lines.First();
        // Create a new line with bad quantity
        var badLine = new ZatcaInvoiceLine(
            Guid.NewGuid(), invoice.Id, "Bad Item", -1, 100m);
        invoice.Lines.Clear();
        invoice.Lines.Add(badLine);
        invoice.SubTotal = badLine.NetAmount;
        invoice.VatAmount = badLine.VatAmount;
        invoice.GrandTotal = badLine.TotalAmount;

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("Quantity must be greater than zero"));
    }

    [Fact]
    public void Line_Without_Item_Name_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        invoice.Lines.Clear();
        var badLine = new ZatcaInvoiceLine(
            Guid.NewGuid(), invoice.Id, "", 1, 100m);
        invoice.Lines.Add(badLine);
        invoice.SubTotal = badLine.NetAmount;
        invoice.VatAmount = badLine.VatAmount;
        invoice.GrandTotal = badLine.TotalAmount;

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("Item name is required"));
    }

    [Fact]
    public void Line_With_Invalid_Tax_Percent_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        var line = invoice.Lines.First();
        line.TaxPercent = 150; // > 100

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("Tax percentage must be between 0 and 100"));
    }

    #endregion

    #region Amount Validation Tests

    [Fact]
    public void Invoice_With_Mismatched_Totals_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        invoice.SubTotal = 999m; // Wrong value

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("subtotal") && e.Contains("does not match"));
    }

    [Fact]
    public void Invoice_With_Negative_Grand_Total_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        invoice.GrandTotal = -100m;

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("grand total must be greater than zero"));
    }

    #endregion

    #region Date Validation Tests

    [Fact]
    public void Invoice_With_Future_Date_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        invoice.IssueDate = DateTime.UtcNow.AddDays(10);

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("cannot be in the future"));
    }

    [Fact]
    public void Invoice_With_Very_Old_Date_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        invoice.IssueDate = DateTime.UtcNow.AddDays(-400);

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("cannot be older than"));
    }

    #endregion

    #region Currency Validation Tests

    [Fact]
    public void Invoice_Without_Currency_Code_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        invoice.CurrencyCode = "";

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("Currency code is required"));
    }

    [Fact]
    public void Invoice_With_Invalid_Currency_Length_Should_Fail()
    {
        var invoice = CreateValidStandardInvoice();
        invoice.CurrencyCode = "ABCD"; // 4 chars

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("3-letter ISO code"));
    }

    #endregion

    #region Invoice Hash Tests

    [Fact]
    public void Invoice_Hash_Should_Be_Deterministic()
    {
        var signer = new ZatcaInvoiceSigner();
        var xml = "<Invoice>test content</Invoice>";

        var hash1 = signer.ComputeInvoiceHash(xml);
        var hash2 = signer.ComputeInvoiceHash(xml);

        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void Different_Xml_Should_Produce_Different_Hash()
    {
        var signer = new ZatcaInvoiceSigner();

        var hash1 = signer.ComputeInvoiceHash("<Invoice>content A</Invoice>");
        var hash2 = signer.ComputeInvoiceHash("<Invoice>content B</Invoice>");

        hash1.ShouldNotBe(hash2);
    }

    [Fact]
    public void Invoice_Hash_Should_Be_Valid_Base64()
    {
        var signer = new ZatcaInvoiceSigner();
        var hash = signer.ComputeInvoiceHash("<Invoice>test</Invoice>");

        hash.ShouldNotBeNullOrWhiteSpace();
        var bytes = Convert.FromBase64String(hash);
        bytes.Length.ShouldBe(32); // SHA-256 = 32 bytes
    }

    #endregion
}
