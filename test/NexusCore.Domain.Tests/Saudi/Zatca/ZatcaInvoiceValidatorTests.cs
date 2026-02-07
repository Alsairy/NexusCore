using System;
using System.Collections.Generic;
using NexusCore.Saudi.Zatca;
using Shouldly;
using Xunit;

namespace NexusCore.Saudi.Tests.Zatca;

public class ZatcaInvoiceValidatorTests
{
    private readonly ZatcaInvoiceValidator _validator = new();

    private ZatcaInvoice CreateValidInvoice()
    {
        var sellerId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        var seller = new ZatcaSeller(sellerId, "شركة الاختبار", "300000000000003")
        {
            City = "الرياض",
            Street = "شارع الأمير",
            PostalCode = "12345"
        };

        var line = new ZatcaInvoiceLine(Guid.NewGuid(), invoiceId, "خدمة استشارية", 2, 500m);

        var invoice = new ZatcaInvoice(invoiceId, sellerId, "INV-001", ZatcaInvoiceType.Standard, DateTime.UtcNow.AddDays(-1))
        {
            BuyerName = "شركة المشتري",
            BuyerVatNumber = "300000000000010",
            CurrencyCode = "SAR",
            SubTotal = line.NetAmount,
            VatAmount = line.VatAmount,
            GrandTotal = line.TotalAmount,
            Seller = seller,
            Lines = new List<ZatcaInvoiceLine> { line }
        };

        return invoice;
    }

    [Fact]
    public void Valid_Invoice_Should_Pass()
    {
        var invoice = CreateValidInvoice();
        var errors = _validator.Validate(invoice);
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Fail_With_Missing_InvoiceNumber()
    {
        var invoice = CreateValidInvoice();
        invoice.InvoiceNumber = "";

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("Invoice number"));
    }

    [Fact]
    public void Should_Fail_With_Invalid_VatFormat()
    {
        var invoice = CreateValidInvoice();
        invoice.Seller.VatRegistrationNumber = "12345"; // Invalid format

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("VAT registration number"));
    }

    [Fact]
    public void Should_Fail_With_Empty_Lines()
    {
        var invoice = CreateValidInvoice();
        invoice.Lines = new List<ZatcaInvoiceLine>();

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("at least one line"));
    }

    [Fact]
    public void Should_Fail_With_Negative_Amount()
    {
        var invoice = CreateValidInvoice();
        var line = invoice.Lines as List<ZatcaInvoiceLine>;
        line![0] = new ZatcaInvoiceLine(Guid.NewGuid(), invoice.Id, "Test", 0, 100m);

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("Quantity must be greater than zero"));
    }

    [Fact]
    public void Should_Fail_With_Future_Date()
    {
        var invoice = CreateValidInvoice();
        invoice.IssueDate = DateTime.UtcNow.AddDays(10);

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("future"));
    }

    [Fact]
    public void Standard_Invoice_Without_Buyer_Should_Fail()
    {
        var invoice = CreateValidInvoice();
        invoice.BuyerName = null;
        invoice.BuyerVatNumber = null;

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("Buyer name is required"));
    }

    [Fact]
    public void Should_Fail_With_Null_Seller()
    {
        var invoice = CreateValidInvoice();
        invoice.Seller = null!;

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("Seller information is required"));
    }

    [Fact]
    public void Should_Fail_With_Amount_Mismatch()
    {
        var invoice = CreateValidInvoice();
        invoice.SubTotal = 999m; // Wrong amount

        var errors = _validator.Validate(invoice);
        errors.ShouldContain(e => e.Contains("subtotal") || e.Contains("grand total"));
    }

    [Fact]
    public void IsValid_Should_Return_True_For_Valid_Invoice()
    {
        var invoice = CreateValidInvoice();
        _validator.IsValid(invoice).ShouldBeTrue();
    }

    [Fact]
    public void IsValid_Should_Return_False_For_Invalid_Invoice()
    {
        var invoice = CreateValidInvoice();
        invoice.InvoiceNumber = "";
        _validator.IsValid(invoice).ShouldBeFalse();
    }
}
