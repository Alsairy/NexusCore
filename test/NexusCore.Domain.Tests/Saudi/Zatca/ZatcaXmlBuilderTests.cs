using System;
using System.Collections.Generic;
using System.Xml.Linq;
using NexusCore.Saudi.Zatca;
using Shouldly;
using Xunit;

namespace NexusCore.Saudi.Tests.Zatca;

public class ZatcaXmlBuilderTests
{
    private readonly ZatcaXmlBuilder _xmlBuilder = new();

    private (ZatcaInvoice invoice, ZatcaSeller seller) CreateTestData()
    {
        var sellerId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        var seller = new ZatcaSeller(sellerId, "شركة الاختبار", "300000000000003")
        {
            SellerNameEn = "Test Company",
            City = "الرياض",
            District = "العليا",
            Street = "شارع الأمير",
            BuildingNumber = "1234",
            PostalCode = "12345",
            CountryCode = "SA",
            CommercialRegistrationNumber = "1234567890"
        };

        var line = new ZatcaInvoiceLine(Guid.NewGuid(), invoiceId, "خدمة استشارية", 1, 1000m);

        var invoice = new ZatcaInvoice(invoiceId, sellerId, "INV-001", ZatcaInvoiceType.Standard, new DateTime(2024, 6, 15))
        {
            BuyerName = "شركة المشتري",
            BuyerVatNumber = "300000000000010",
            CurrencyCode = "SAR",
            SubTotal = 1000m,
            VatAmount = 150m,
            GrandTotal = 1150m,
            Seller = seller,
            Lines = new List<ZatcaInvoiceLine> { line }
        };

        return (invoice, seller);
    }

    [Fact]
    public void Should_Build_Valid_Xml()
    {
        var (invoice, seller) = CreateTestData();

        var xml = _xmlBuilder.BuildInvoiceXml(invoice, seller, "test-hash", "test-qr");

        xml.ShouldNotBeNullOrWhiteSpace();
        // Should be parseable XML
        var doc = XDocument.Parse(xml);
        doc.ShouldNotBeNull();
    }

    [Fact]
    public void Should_Have_Correct_Namespaces()
    {
        var (invoice, seller) = CreateTestData();
        var xml = _xmlBuilder.BuildInvoiceXml(invoice, seller, "test-hash", "test-qr");
        var doc = XDocument.Parse(xml);

        var root = doc.Root!;
        root.Name.NamespaceName.ShouldBe("urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
    }

    [Fact]
    public void Should_Contain_SupplierParty()
    {
        var (invoice, seller) = CreateTestData();
        var xml = _xmlBuilder.BuildInvoiceXml(invoice, seller, "test-hash", "test-qr");

        xml.ShouldContain("AccountingSupplierParty");
        xml.ShouldContain(seller.SellerNameAr);
        xml.ShouldContain(seller.VatRegistrationNumber);
    }

    [Fact]
    public void Should_Contain_BuyerParty_For_Standard_Invoice()
    {
        var (invoice, seller) = CreateTestData();
        var xml = _xmlBuilder.BuildInvoiceXml(invoice, seller, "test-hash", "test-qr");

        xml.ShouldContain("AccountingCustomerParty");
        xml.ShouldContain(invoice.BuyerName!);
        xml.ShouldContain(invoice.BuyerVatNumber!);
    }

    [Fact]
    public void Should_Include_Invoice_Type_Code()
    {
        var (invoice, seller) = CreateTestData();
        var xml = _xmlBuilder.BuildInvoiceXml(invoice, seller, "test-hash", "test-qr");

        xml.ShouldContain("InvoiceTypeCode");
        xml.ShouldContain("388"); // Standard invoice type code
    }

    [Fact]
    public void Should_Include_QR_Code_In_Extensions()
    {
        var (invoice, seller) = CreateTestData();
        var xml = _xmlBuilder.BuildInvoiceXml(invoice, seller, "test-hash", "test-qr");

        xml.ShouldContain("test-qr");
        xml.ShouldContain("urn:zatca:ksa:qr");
    }

    [Fact]
    public void Should_Include_Invoice_Hash()
    {
        var (invoice, seller) = CreateTestData();
        var xml = _xmlBuilder.BuildInvoiceXml(invoice, seller, "test-hash", "test-qr");

        xml.ShouldContain("ICV");
        xml.ShouldContain("test-hash");
    }

    [Fact]
    public void Simplified_Invoice_Should_Not_Require_Buyer()
    {
        var (invoice, seller) = CreateTestData();
        invoice = new ZatcaInvoice(invoice.Id, invoice.SellerId, "INV-002", ZatcaInvoiceType.Simplified, new DateTime(2024, 6, 15))
        {
            CurrencyCode = "SAR",
            SubTotal = 1000m,
            VatAmount = 150m,
            GrandTotal = 1150m,
            Seller = seller,
            Lines = invoice.Lines
        };

        var xml = _xmlBuilder.BuildInvoiceXml(invoice, seller, "test-hash", "test-qr");

        xml.ShouldNotBeNullOrWhiteSpace();
        xml.ShouldNotContain("AccountingCustomerParty");
    }
}
