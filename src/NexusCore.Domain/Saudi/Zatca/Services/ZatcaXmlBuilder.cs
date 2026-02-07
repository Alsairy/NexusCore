using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Volo.Abp.DependencyInjection;

namespace NexusCore.Saudi.Zatca;

/// <summary>
/// Builds UBL 2.1 XML documents for ZATCA e-invoices according to the specification.
/// </summary>
public class ZatcaXmlBuilder : ITransientDependency
{
    private static readonly XNamespace Invoice2Ns = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
    private static readonly XNamespace CacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace CbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace ExtNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";

    /// <summary>
    /// Builds a UBL 2.1 XML document for a ZATCA invoice.
    /// </summary>
    /// <param name="invoice">The invoice entity with all lines loaded</param>
    /// <param name="seller">The seller entity</param>
    /// <param name="invoiceHash">The computed invoice hash (SHA-256 Base64)</param>
    /// <param name="qrCode">The generated QR code (Base64 TLV)</param>
    /// <returns>XML string formatted for ZATCA submission</returns>
    public string BuildInvoiceXml(
        ZatcaInvoice invoice,
        ZatcaSeller seller,
        string invoiceHash,
        string qrCode)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            CreateInvoiceElement(invoice, seller, invoiceHash, qrCode)
        );

        return doc.ToString(SaveOptions.None);
    }

    private XElement CreateInvoiceElement(
        ZatcaInvoice invoice,
        ZatcaSeller seller,
        string invoiceHash,
        string qrCode)
    {
        var root = new XElement(Invoice2Ns + "Invoice",
            new XAttribute(XNamespace.Xmlns + "cac", CacNs),
            new XAttribute(XNamespace.Xmlns + "cbc", CbcNs),
            new XAttribute(XNamespace.Xmlns + "ext", ExtNs));

        // UBL Extensions (for signatures and QR)
        root.Add(CreateUblExtensions(qrCode));

        // Invoice identification
        root.Add(new XElement(CbcNs + "ID", invoice.InvoiceNumber));
        root.Add(new XElement(CbcNs + "UUID", invoice.Id.ToString()));
        root.Add(new XElement(CbcNs + "IssueDate", invoice.IssueDate.ToString("yyyy-MM-dd")));
        root.Add(new XElement(CbcNs + "IssueTime", invoice.IssueDate.ToString("HH:mm:ss")));

        // Invoice type code
        root.Add(new XElement(CbcNs + "InvoiceTypeCode",
            new XAttribute("name", GetInvoiceTypeName(invoice.InvoiceType)),
            ((int)invoice.InvoiceType).ToString()));

        // Document currency code
        root.Add(new XElement(CbcNs + "DocumentCurrencyCode", invoice.CurrencyCode));

        // Tax currency code (SAR)
        root.Add(new XElement(CbcNs + "TaxCurrencyCode", ZatcaConsts.DefaultCurrencyCode));

        // Invoice hash (PIH - Previous Invoice Hash)
        if (!string.IsNullOrWhiteSpace(invoice.PreviousInvoiceHash))
        {
            root.Add(new XElement(CacNs + "AdditionalDocumentReference",
                new XElement(CbcNs + "ID", "PIH"),
                new XElement(CacNs + "Attachment",
                    new XElement(CbcNs + "EmbeddedDocumentBinaryObject",
                        new XAttribute("mimeCode", "text/plain"),
                        invoice.PreviousInvoiceHash))));
        }

        // Current invoice hash (ICV - Invoice Counter Value)
        root.Add(new XElement(CacNs + "AdditionalDocumentReference",
            new XElement(CbcNs + "ID", "ICV"),
            new XElement(CacNs + "Attachment",
                new XElement(CbcNs + "EmbeddedDocumentBinaryObject",
                    new XAttribute("mimeCode", "text/plain"),
                    invoiceHash))));

        // Accounting supplier party (seller)
        root.Add(CreateSupplierParty(seller));

        // Accounting customer party (buyer) - if B2B
        if (!string.IsNullOrWhiteSpace(invoice.BuyerName))
        {
            root.Add(CreateCustomerParty(invoice));
        }

        // Tax total
        root.Add(CreateTaxTotal(invoice));

        // Legal monetary total
        root.Add(CreateLegalMonetaryTotal(invoice));

        // Invoice lines
        foreach (var line in invoice.Lines.OrderBy(l => l.CreationTime))
        {
            root.Add(CreateInvoiceLine(line));
        }

        return root;
    }

    private XElement CreateUblExtensions(string qrCode)
    {
        return new XElement(ExtNs + "UBLExtensions",
            new XElement(ExtNs + "UBLExtension",
                new XElement(ExtNs + "ExtensionURI", "urn:oasis:names:specification:ubl:dsig:enveloped:xades"),
                new XElement(ExtNs + "ExtensionContent",
                    new XComment(" UBL Signature will be inserted here "))),
            new XElement(ExtNs + "UBLExtension",
                new XElement(ExtNs + "ExtensionURI", "urn:zatca:ksa:qr"),
                new XElement(ExtNs + "ExtensionContent",
                    new XElement("QRCode", qrCode))));
    }

    private XElement CreateSupplierParty(ZatcaSeller seller)
    {
        var party = new XElement(CacNs + "AccountingSupplierParty",
            new XElement(CacNs + "Party",
                new XElement(CacNs + "PartyIdentification",
                    new XElement(CbcNs + "ID",
                        new XAttribute("schemeID", "CRN"),
                        seller.CommercialRegistrationNumber ?? "")),
                new XElement(CacNs + "PostalAddress",
                    new XElement(CbcNs + "StreetName", seller.Street ?? ""),
                    new XElement(CbcNs + "BuildingNumber", seller.BuildingNumber ?? ""),
                    new XElement(CbcNs + "CityName", seller.City ?? ""),
                    new XElement(CbcNs + "PostalZone", seller.PostalCode ?? ""),
                    new XElement(CbcNs + "CountrySubentity", seller.District ?? ""),
                    new XElement(CacNs + "Country",
                        new XElement(CbcNs + "IdentificationCode", seller.CountryCode ?? ZatcaConsts.DefaultCountryCode))),
                new XElement(CacNs + "PartyTaxScheme",
                    new XElement(CbcNs + "CompanyID", seller.VatRegistrationNumber),
                    new XElement(CacNs + "TaxScheme",
                        new XElement(CbcNs + "ID", "VAT"))),
                new XElement(CacNs + "PartyLegalEntity",
                    new XElement(CbcNs + "RegistrationName", seller.SellerNameAr))));

        return party;
    }

    private XElement CreateCustomerParty(ZatcaInvoice invoice)
    {
        var party = new XElement(CacNs + "AccountingCustomerParty",
            new XElement(CacNs + "Party",
                new XElement(CacNs + "PartyLegalEntity",
                    new XElement(CbcNs + "RegistrationName", invoice.BuyerName ?? ""))));

        if (!string.IsNullOrWhiteSpace(invoice.BuyerVatNumber))
        {
            party.Element(CacNs + "Party")?.Add(
                new XElement(CacNs + "PartyTaxScheme",
                    new XElement(CbcNs + "CompanyID", invoice.BuyerVatNumber),
                    new XElement(CacNs + "TaxScheme",
                        new XElement(CbcNs + "ID", "VAT"))));
        }

        return party;
    }

    private XElement CreateTaxTotal(ZatcaInvoice invoice)
    {
        return new XElement(CacNs + "TaxTotal",
            new XElement(CbcNs + "TaxAmount",
                new XAttribute("currencyID", invoice.CurrencyCode),
                invoice.VatAmount.ToString("0.00", CultureInfo.InvariantCulture)),
            new XElement(CacNs + "TaxSubtotal",
                new XElement(CbcNs + "TaxableAmount",
                    new XAttribute("currencyID", invoice.CurrencyCode),
                    invoice.SubTotal.ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement(CbcNs + "TaxAmount",
                    new XAttribute("currencyID", invoice.CurrencyCode),
                    invoice.VatAmount.ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement(CacNs + "TaxCategory",
                    new XElement(CbcNs + "ID", "S"),
                    new XElement(CbcNs + "Percent", ZatcaConsts.DefaultVatRate.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement(CacNs + "TaxScheme",
                        new XElement(CbcNs + "ID", "VAT")))));
    }

    private XElement CreateLegalMonetaryTotal(ZatcaInvoice invoice)
    {
        return new XElement(CacNs + "LegalMonetaryTotal",
            new XElement(CbcNs + "LineExtensionAmount",
                new XAttribute("currencyID", invoice.CurrencyCode),
                invoice.SubTotal.ToString("0.00", CultureInfo.InvariantCulture)),
            new XElement(CbcNs + "TaxExclusiveAmount",
                new XAttribute("currencyID", invoice.CurrencyCode),
                invoice.SubTotal.ToString("0.00", CultureInfo.InvariantCulture)),
            new XElement(CbcNs + "TaxInclusiveAmount",
                new XAttribute("currencyID", invoice.CurrencyCode),
                invoice.GrandTotal.ToString("0.00", CultureInfo.InvariantCulture)),
            new XElement(CbcNs + "PayableAmount",
                new XAttribute("currencyID", invoice.CurrencyCode),
                invoice.GrandTotal.ToString("0.00", CultureInfo.InvariantCulture)));
    }

    private XElement CreateInvoiceLine(ZatcaInvoiceLine line)
    {
        return new XElement(CacNs + "InvoiceLine",
            new XElement(CbcNs + "ID", line.Id.ToString()),
            new XElement(CbcNs + "InvoicedQuantity",
                new XAttribute("unitCode", "PCE"),
                line.Quantity.ToString("0.00", CultureInfo.InvariantCulture)),
            new XElement(CbcNs + "LineExtensionAmount",
                new XAttribute("currencyID", ZatcaConsts.DefaultCurrencyCode),
                line.NetAmount.ToString("0.00", CultureInfo.InvariantCulture)),
            new XElement(CacNs + "TaxTotal",
                new XElement(CbcNs + "TaxAmount",
                    new XAttribute("currencyID", ZatcaConsts.DefaultCurrencyCode),
                    line.VatAmount.ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement(CbcNs + "RoundingAmount",
                    new XAttribute("currencyID", ZatcaConsts.DefaultCurrencyCode),
                    line.TotalAmount.ToString("0.00", CultureInfo.InvariantCulture))),
            new XElement(CacNs + "Item",
                new XElement(CbcNs + "Name", line.ItemName),
                new XElement(CacNs + "ClassifiedTaxCategory",
                    new XElement(CbcNs + "ID", line.TaxCategoryCode ?? "S"),
                    new XElement(CbcNs + "Percent", line.TaxPercent.ToString("0.00", CultureInfo.InvariantCulture)),
                    new XElement(CacNs + "TaxScheme",
                        new XElement(CbcNs + "ID", "VAT")))),
            new XElement(CacNs + "Price",
                new XElement(CbcNs + "PriceAmount",
                    new XAttribute("currencyID", ZatcaConsts.DefaultCurrencyCode),
                    line.UnitPrice.ToString("0.00", CultureInfo.InvariantCulture))));
    }

    private string GetInvoiceTypeName(ZatcaInvoiceType invoiceType)
    {
        return invoiceType switch
        {
            ZatcaInvoiceType.Standard => "Standard Tax Invoice",
            ZatcaInvoiceType.Simplified => "Simplified Tax Invoice",
            ZatcaInvoiceType.StandardDebitNote => "Standard Debit Note",
            ZatcaInvoiceType.StandardCreditNote => "Standard Credit Note",
            ZatcaInvoiceType.SimplifiedDebitNote => "Simplified Debit Note",
            ZatcaInvoiceType.SimplifiedCreditNote => "Simplified Credit Note",
            _ => "Tax Invoice"
        };
    }
}
