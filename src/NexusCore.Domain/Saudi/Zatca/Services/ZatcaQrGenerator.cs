using System;
using System.IO;
using System.Text;
using Volo.Abp.DependencyInjection;

namespace NexusCore.Saudi.Zatca;

/// <summary>
/// Generates ZATCA-compliant QR codes using TLV (Tag-Length-Value) encoding.
/// Implements Phase 2 specification for e-invoice QR codes.
/// </summary>
public class ZatcaQrGenerator : ITransientDependency
{
    private const byte SellerNameTag = 1;
    private const byte VatRegistrationNumberTag = 2;
    private const byte InvoiceTimestampTag = 3;
    private const byte InvoiceTotalTag = 4;
    private const byte VatTotalTag = 5;

    /// <summary>
    /// Generates a Base64-encoded QR code for a ZATCA invoice.
    /// </summary>
    /// <param name="sellerName">Seller's name in Arabic</param>
    /// <param name="vatRegistrationNumber">Seller's VAT registration number</param>
    /// <param name="invoiceTimestamp">Invoice issue date and time</param>
    /// <param name="invoiceTotal">Invoice grand total with VAT</param>
    /// <param name="vatTotal">Total VAT amount</param>
    /// <returns>Base64-encoded TLV QR code string</returns>
    public string GenerateQrCode(
        string sellerName,
        string vatRegistrationNumber,
        DateTime invoiceTimestamp,
        decimal invoiceTotal,
        decimal vatTotal)
    {
        using var stream = new MemoryStream();

        // Tag 1: Seller Name
        AppendTlv(stream, SellerNameTag, sellerName);

        // Tag 2: VAT Registration Number
        AppendTlv(stream, VatRegistrationNumberTag, vatRegistrationNumber);

        // Tag 3: Invoice Timestamp (ISO 8601 format)
        var timestampString = invoiceTimestamp.ToString("yyyy-MM-ddTHH:mm:ssZ");
        AppendTlv(stream, InvoiceTimestampTag, timestampString);

        // Tag 4: Invoice Total (with VAT)
        AppendTlv(stream, InvoiceTotalTag, invoiceTotal.ToString("0.00"));

        // Tag 5: VAT Total
        AppendTlv(stream, VatTotalTag, vatTotal.ToString("0.00"));

        var tlvBytes = stream.ToArray();
        return Convert.ToBase64String(tlvBytes);
    }

    /// <summary>
    /// Appends a TLV (Tag-Length-Value) entry to the stream.
    /// </summary>
    private void AppendTlv(MemoryStream stream, byte tag, string value)
    {
        var valueBytes = Encoding.UTF8.GetBytes(value);
        var length = (byte)valueBytes.Length;

        stream.WriteByte(tag);
        stream.WriteByte(length);
        stream.Write(valueBytes, 0, valueBytes.Length);
    }
}
