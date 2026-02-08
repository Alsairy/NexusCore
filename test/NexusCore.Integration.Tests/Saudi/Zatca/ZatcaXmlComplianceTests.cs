using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NexusCore.Saudi.Zatca;
using Shouldly;
using Xunit;

namespace NexusCore.Saudi.Tests.Zatca;

/// <summary>
/// Offline XML compliance tests that validate generated invoice XML
/// against ZATCA UBL 2.1 requirements. These tests do not require
/// network access — they verify the structural correctness of XML output.
/// </summary>
public class ZatcaXmlComplianceTests
{
    private static readonly XNamespace Invoice2Ns = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
    private static readonly XNamespace CacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace CbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace ExtNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";

    private readonly ZatcaXmlBuilder _xmlBuilder = new();
    private readonly ZatcaQrGenerator _qrGenerator = new();
    private readonly ZatcaInvoiceSigner _signer = new();
    private readonly ZatcaInvoiceValidator _validator = new();

    #region Helper Methods

    private static ZatcaSeller CreateTestSeller()
    {
        return new ZatcaSeller(
            Guid.NewGuid(),
            "شركة الاختبار للتجارة",
            "300000000000003")
        {
            SellerNameEn = "Test Trading Company",
            CommercialRegistrationNumber = "1234567890",
            Street = "شارع الأمير محمد بن عبدالعزيز",
            BuildingNumber = "1234",
            City = "الرياض",
            District = "العليا",
            PostalCode = "12345",
            CountryCode = "SA",
            IsDefault = true
        };
    }

    private static ZatcaInvoice CreateStandardInvoice(ZatcaSeller seller)
    {
        var invoice = new ZatcaInvoice(
            Guid.NewGuid(),
            seller.Id,
            "INV-2024-001",
            ZatcaInvoiceType.Standard,
            new DateTime(2024, 6, 15, 10, 30, 0))
        {
            BuyerName = "شركة المشتري",
            BuyerVatNumber = "300000000000010",
            CurrencyCode = "SAR",
            Seller = seller
        };

        var line1 = new ZatcaInvoiceLine(
            Guid.NewGuid(), invoice.Id, "خدمة استشارية", 2, 1000m);
        var line2 = new ZatcaInvoiceLine(
            Guid.NewGuid(), invoice.Id, "تدريب فني", 1, 500m);

        invoice.Lines.Add(line1);
        invoice.Lines.Add(line2);

        invoice.SubTotal = line1.NetAmount + line2.NetAmount;
        invoice.VatAmount = line1.VatAmount + line2.VatAmount;
        invoice.GrandTotal = line1.TotalAmount + line2.TotalAmount;

        return invoice;
    }

    private static ZatcaInvoice CreateSimplifiedInvoice(ZatcaSeller seller)
    {
        var invoice = new ZatcaInvoice(
            Guid.NewGuid(),
            seller.Id,
            "SINV-2024-001",
            ZatcaInvoiceType.Simplified,
            new DateTime(2024, 7, 1, 14, 0, 0))
        {
            BuyerName = "عميل نقدي",
            CurrencyCode = "SAR",
            Seller = seller
        };

        var line = new ZatcaInvoiceLine(
            Guid.NewGuid(), invoice.Id, "منتج اختبار", 3, 100m);

        invoice.Lines.Add(line);
        invoice.SubTotal = line.NetAmount;
        invoice.VatAmount = line.VatAmount;
        invoice.GrandTotal = line.TotalAmount;

        return invoice;
    }

    private string BuildTestXml(ZatcaInvoice invoice, ZatcaSeller seller)
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

    #endregion

    #region UBL 2.1 Namespace Tests

    [Fact]
    public void Xml_Should_Have_Invoice2_Root_Namespace()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        doc.Root.ShouldNotBeNull();
        doc.Root.Name.Namespace.ShouldBe(Invoice2Ns);
        doc.Root.Name.LocalName.ShouldBe("Invoice");
    }

    [Fact]
    public void Xml_Should_Declare_All_Mandatory_Namespaces()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var root = doc.Root!;

        var namespaces = root.Attributes()
            .Where(a => a.IsNamespaceDeclaration)
            .ToDictionary(a => a.Name.LocalName, a => a.Value);

        namespaces.ShouldContainKey("cac");
        namespaces["cac"].ShouldBe(CacNs.NamespaceName);

        namespaces.ShouldContainKey("cbc");
        namespaces["cbc"].ShouldBe(CbcNs.NamespaceName);

        namespaces.ShouldContainKey("ext");
        namespaces["ext"].ShouldBe(ExtNs.NamespaceName);
    }

    #endregion

    #region Invoice Identification Tests

    [Fact]
    public void Xml_Should_Contain_Invoice_Number_As_ID()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var id = doc.Root!.Element(CbcNs + "ID");

        id.ShouldNotBeNull();
        id.Value.ShouldBe("INV-2024-001");
    }

    [Fact]
    public void Xml_Should_Contain_Valid_UUID()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var uuid = doc.Root!.Element(CbcNs + "UUID");

        uuid.ShouldNotBeNull();
        Guid.TryParse(uuid.Value, out var parsed).ShouldBeTrue();
        parsed.ShouldBe(invoice.Id);
    }

    [Fact]
    public void Xml_Should_Contain_Issue_Date_In_Correct_Format()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var issueDate = doc.Root!.Element(CbcNs + "IssueDate");
        var issueTime = doc.Root!.Element(CbcNs + "IssueTime");

        issueDate.ShouldNotBeNull();
        // The XML builder uses the current culture's calendar (may be Hijri or Gregorian)
        var expectedDate = invoice.IssueDate.ToString("yyyy-MM-dd");
        issueDate.Value.ShouldBe(expectedDate);

        issueTime.ShouldNotBeNull();
        issueTime.Value.ShouldBe("10:30:00");
    }

    [Fact]
    public void Xml_Should_Have_Correct_InvoiceTypeCode_For_Standard()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var typeCode = doc.Root!.Element(CbcNs + "InvoiceTypeCode");

        typeCode.ShouldNotBeNull();
        typeCode.Value.ShouldBe("388"); // Standard = 388
        typeCode.Attribute("name")!.Value.ShouldBe("Standard Tax Invoice");
    }

    [Fact]
    public void Xml_Should_Have_Correct_InvoiceTypeCode_For_Simplified()
    {
        var seller = CreateTestSeller();
        var invoice = CreateSimplifiedInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var typeCode = doc.Root!.Element(CbcNs + "InvoiceTypeCode");

        typeCode.ShouldNotBeNull();
        typeCode.Value.ShouldBe("383"); // Simplified = 383
        typeCode.Attribute("name")!.Value.ShouldBe("Simplified Tax Invoice");
    }

    #endregion

    #region Currency Tests

    [Fact]
    public void Xml_Should_Contain_Document_And_Tax_Currency_Codes()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);

        var docCurrency = doc.Root!.Element(CbcNs + "DocumentCurrencyCode");
        docCurrency.ShouldNotBeNull();
        docCurrency.Value.ShouldBe("SAR");

        var taxCurrency = doc.Root!.Element(CbcNs + "TaxCurrencyCode");
        taxCurrency.ShouldNotBeNull();
        taxCurrency.Value.ShouldBe("SAR");
    }

    #endregion

    #region Supplier Party Tests

    [Fact]
    public void Xml_Should_Contain_Supplier_Party_With_Arabic_Name()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var supplierParty = doc.Root!.Element(CacNs + "AccountingSupplierParty");

        supplierParty.ShouldNotBeNull();

        var party = supplierParty.Element(CacNs + "Party");
        party.ShouldNotBeNull();

        // Registration name should be Arabic
        var regName = party
            .Element(CacNs + "PartyLegalEntity")?
            .Element(CbcNs + "RegistrationName");
        regName.ShouldNotBeNull();
        regName.Value.ShouldBe("شركة الاختبار للتجارة");
    }

    [Fact]
    public void Xml_Should_Contain_Supplier_VAT_Number()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var partyTaxScheme = doc.Root!
            .Element(CacNs + "AccountingSupplierParty")?
            .Element(CacNs + "Party")?
            .Element(CacNs + "PartyTaxScheme");

        partyTaxScheme.ShouldNotBeNull();

        var companyId = partyTaxScheme.Element(CbcNs + "CompanyID");
        companyId.ShouldNotBeNull();
        companyId.Value.ShouldBe("300000000000003");

        var taxSchemeId = partyTaxScheme
            .Element(CacNs + "TaxScheme")?
            .Element(CbcNs + "ID");
        taxSchemeId.ShouldNotBeNull();
        taxSchemeId.Value.ShouldBe("VAT");
    }

    [Fact]
    public void Xml_Should_Contain_Supplier_CRN()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var partyId = doc.Root!
            .Element(CacNs + "AccountingSupplierParty")?
            .Element(CacNs + "Party")?
            .Element(CacNs + "PartyIdentification")?
            .Element(CbcNs + "ID");

        partyId.ShouldNotBeNull();
        partyId.Attribute("schemeID")!.Value.ShouldBe("CRN");
        partyId.Value.ShouldBe("1234567890");
    }

    [Fact]
    public void Xml_Should_Contain_Supplier_Postal_Address()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var postalAddress = doc.Root!
            .Element(CacNs + "AccountingSupplierParty")?
            .Element(CacNs + "Party")?
            .Element(CacNs + "PostalAddress");

        postalAddress.ShouldNotBeNull();

        postalAddress.Element(CbcNs + "StreetName")!.Value.ShouldBe("شارع الأمير محمد بن عبدالعزيز");
        postalAddress.Element(CbcNs + "BuildingNumber")!.Value.ShouldBe("1234");
        postalAddress.Element(CbcNs + "CityName")!.Value.ShouldBe("الرياض");
        postalAddress.Element(CbcNs + "PostalZone")!.Value.ShouldBe("12345");
        postalAddress.Element(CbcNs + "CountrySubentity")!.Value.ShouldBe("العليا");

        var country = postalAddress.Element(CacNs + "Country");
        country.ShouldNotBeNull();
        country.Element(CbcNs + "IdentificationCode")!.Value.ShouldBe("SA");
    }

    #endregion

    #region Customer Party Tests

    [Fact]
    public void Standard_Invoice_Should_Have_Customer_Party()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var customerParty = doc.Root!.Element(CacNs + "AccountingCustomerParty");

        customerParty.ShouldNotBeNull();

        var regName = customerParty
            .Element(CacNs + "Party")?
            .Element(CacNs + "PartyLegalEntity")?
            .Element(CbcNs + "RegistrationName");
        regName.ShouldNotBeNull();
        regName.Value.ShouldBe("شركة المشتري");
    }

    [Fact]
    public void Standard_Invoice_Should_Have_Buyer_VAT_Number()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var buyerTaxScheme = doc.Root!
            .Element(CacNs + "AccountingCustomerParty")?
            .Element(CacNs + "Party")?
            .Element(CacNs + "PartyTaxScheme");

        buyerTaxScheme.ShouldNotBeNull();
        buyerTaxScheme.Element(CbcNs + "CompanyID")!.Value.ShouldBe("300000000000010");
    }

    [Fact]
    public void Simplified_Invoice_Without_Buyer_VAT_Should_Omit_PartyTaxScheme()
    {
        var seller = CreateTestSeller();
        var invoice = CreateSimplifiedInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var customerParty = doc.Root!.Element(CacNs + "AccountingCustomerParty");

        // Simplified invoice has buyer name so customer party exists
        customerParty.ShouldNotBeNull();

        // But no BuyerVatNumber → no PartyTaxScheme
        var buyerTaxScheme = customerParty
            .Element(CacNs + "Party")?
            .Element(CacNs + "PartyTaxScheme");
        buyerTaxScheme.ShouldBeNull();
    }

    #endregion

    #region Tax Total Tests

    [Fact]
    public void Xml_Should_Contain_Tax_Total_Matching_Line_Items()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var taxTotal = doc.Root!.Element(CacNs + "TaxTotal");
        taxTotal.ShouldNotBeNull();

        // TaxAmount should match invoice.VatAmount
        var taxAmount = taxTotal.Element(CbcNs + "TaxAmount");
        taxAmount.ShouldNotBeNull();
        taxAmount.Attribute("currencyID")!.Value.ShouldBe("SAR");
        decimal.Parse(taxAmount.Value, CultureInfo.InvariantCulture)
            .ShouldBe(invoice.VatAmount);

        // TaxSubtotal
        var taxSubtotal = taxTotal.Element(CacNs + "TaxSubtotal");
        taxSubtotal.ShouldNotBeNull();

        var taxableAmount = taxSubtotal.Element(CbcNs + "TaxableAmount");
        taxableAmount.ShouldNotBeNull();
        decimal.Parse(taxableAmount.Value, CultureInfo.InvariantCulture)
            .ShouldBe(invoice.SubTotal);

        // Tax category should be "S" (Standard)
        var taxCategory = taxSubtotal.Element(CacNs + "TaxCategory");
        taxCategory.ShouldNotBeNull();
        taxCategory.Element(CbcNs + "ID")!.Value.ShouldBe("S");
        taxCategory.Element(CbcNs + "Percent")!.Value.ShouldBe("15.00");
    }

    #endregion

    #region Legal Monetary Total Tests

    [Fact]
    public void Xml_Should_Contain_Legal_Monetary_Total()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var legalTotal = doc.Root!.Element(CacNs + "LegalMonetaryTotal");
        legalTotal.ShouldNotBeNull();

        var lineExtension = decimal.Parse(
            legalTotal.Element(CbcNs + "LineExtensionAmount")!.Value,
            CultureInfo.InvariantCulture);
        lineExtension.ShouldBe(invoice.SubTotal);

        var taxExclusive = decimal.Parse(
            legalTotal.Element(CbcNs + "TaxExclusiveAmount")!.Value,
            CultureInfo.InvariantCulture);
        taxExclusive.ShouldBe(invoice.SubTotal);

        var taxInclusive = decimal.Parse(
            legalTotal.Element(CbcNs + "TaxInclusiveAmount")!.Value,
            CultureInfo.InvariantCulture);
        taxInclusive.ShouldBe(invoice.GrandTotal);

        var payable = decimal.Parse(
            legalTotal.Element(CbcNs + "PayableAmount")!.Value,
            CultureInfo.InvariantCulture);
        payable.ShouldBe(invoice.GrandTotal);

        // Grand total should equal SubTotal + VatAmount
        taxInclusive.ShouldBe(lineExtension + invoice.VatAmount);
    }

    #endregion

    #region Invoice Lines Tests

    [Fact]
    public void Xml_Should_Contain_All_Invoice_Lines()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var lines = doc.Root!.Elements(CacNs + "InvoiceLine").ToList();

        lines.Count.ShouldBe(2);
    }

    [Fact]
    public void Invoice_Lines_Should_Have_Correct_Structure()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var firstLine = doc.Root!.Elements(CacNs + "InvoiceLine").First();

        // ID
        firstLine.Element(CbcNs + "ID").ShouldNotBeNull();

        // InvoicedQuantity with unitCode
        var quantity = firstLine.Element(CbcNs + "InvoicedQuantity");
        quantity.ShouldNotBeNull();
        quantity.Attribute("unitCode")!.Value.ShouldBe("PCE");

        // LineExtensionAmount with currencyID
        var lineAmount = firstLine.Element(CbcNs + "LineExtensionAmount");
        lineAmount.ShouldNotBeNull();
        lineAmount.Attribute("currencyID")!.Value.ShouldBe("SAR");

        // TaxTotal
        var lineTax = firstLine.Element(CacNs + "TaxTotal");
        lineTax.ShouldNotBeNull();
        lineTax.Element(CbcNs + "TaxAmount").ShouldNotBeNull();
        lineTax.Element(CbcNs + "RoundingAmount").ShouldNotBeNull();

        // Item with Name and ClassifiedTaxCategory
        var item = firstLine.Element(CacNs + "Item");
        item.ShouldNotBeNull();
        item.Element(CbcNs + "Name").ShouldNotBeNull();

        var taxCategory = item.Element(CacNs + "ClassifiedTaxCategory");
        taxCategory.ShouldNotBeNull();
        taxCategory.Element(CbcNs + "ID")!.Value.ShouldBe("S");
        taxCategory.Element(CbcNs + "Percent")!.Value.ShouldBe("15.00");
        taxCategory.Element(CacNs + "TaxScheme")!
            .Element(CbcNs + "ID")!.Value.ShouldBe("VAT");

        // Price
        var price = firstLine.Element(CacNs + "Price");
        price.ShouldNotBeNull();
        var priceAmount = price.Element(CbcNs + "PriceAmount");
        priceAmount.ShouldNotBeNull();
        priceAmount.Attribute("currencyID")!.Value.ShouldBe("SAR");
    }

    #endregion

    #region UBL Extensions Tests

    [Fact]
    public void Xml_Should_Contain_UBL_Extensions_With_Signature_Placeholder()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var extensions = doc.Root!.Element(ExtNs + "UBLExtensions");
        extensions.ShouldNotBeNull();

        var ublExtensions = extensions.Elements(ExtNs + "UBLExtension").ToList();
        ublExtensions.Count.ShouldBe(2);

        // First extension: signature placeholder
        var sigExtUri = ublExtensions[0].Element(ExtNs + "ExtensionURI");
        sigExtUri.ShouldNotBeNull();
        sigExtUri.Value.ShouldBe("urn:oasis:names:specification:ubl:dsig:enveloped:xades");

        // Second extension: QR code
        var qrExtUri = ublExtensions[1].Element(ExtNs + "ExtensionURI");
        qrExtUri.ShouldNotBeNull();
        qrExtUri.Value.ShouldBe("urn:zatca:ksa:qr");
    }

    [Fact]
    public void Xml_Should_Contain_QR_Code_In_Extension()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var qrExtension = doc.Root!
            .Element(ExtNs + "UBLExtensions")?
            .Elements(ExtNs + "UBLExtension")
            .Last();

        qrExtension.ShouldNotBeNull();

        var qrContent = qrExtension
            .Element(ExtNs + "ExtensionContent")?
            .Element("QRCode");
        qrContent.ShouldNotBeNull();

        // QR code should be a valid Base64 string
        var qrBase64 = qrContent.Value;
        qrBase64.ShouldNotBeNullOrWhiteSpace();
        Convert.FromBase64String(qrBase64).ShouldNotBeEmpty();
    }

    #endregion

    #region Additional Document Reference Tests

    [Fact]
    public void Xml_Should_Contain_ICV_Document_Reference()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var docRefs = doc.Root!
            .Elements(CacNs + "AdditionalDocumentReference")
            .ToList();

        var icvRef = docRefs.FirstOrDefault(r =>
            r.Element(CbcNs + "ID")?.Value == "ICV");

        icvRef.ShouldNotBeNull();

        var attachment = icvRef
            .Element(CacNs + "Attachment")?
            .Element(CbcNs + "EmbeddedDocumentBinaryObject");
        attachment.ShouldNotBeNull();
        attachment.Attribute("mimeCode")!.Value.ShouldBe("text/plain");
        attachment.Value.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Xml_With_Previous_Hash_Should_Contain_PIH_Reference()
    {
        var seller = CreateTestSeller();
        var invoice = CreateStandardInvoice(seller);
        invoice.PreviousInvoiceHash = "dGVzdC1wcmV2aW91cy1oYXNo";

        var xml = BuildTestXml(invoice, seller);

        var doc = XDocument.Parse(xml);
        var pihRef = doc.Root!
            .Elements(CacNs + "AdditionalDocumentReference")
            .FirstOrDefault(r => r.Element(CbcNs + "ID")?.Value == "PIH");

        pihRef.ShouldNotBeNull();

        var attachment = pihRef
            .Element(CacNs + "Attachment")?
            .Element(CbcNs + "EmbeddedDocumentBinaryObject");
        attachment.ShouldNotBeNull();
        attachment.Value.ShouldBe("dGVzdC1wcmV2aW91cy1oYXNo");
    }

    #endregion
}
