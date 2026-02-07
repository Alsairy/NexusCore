using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace NexusCore.Saudi.Zatca;

/// <summary>
/// Domain service that orchestrates the complete ZATCA invoice preparation and submission workflow.
/// Coordinates validation, QR generation, XML building, signing, and hashing.
/// </summary>
public class ZatcaInvoiceManager : DomainService
{
    private readonly ZatcaInvoiceValidator _invoiceValidator;
    private readonly ZatcaQrGenerator _qrGenerator;
    private readonly ZatcaXmlBuilder _xmlBuilder;
    private readonly ZatcaInvoiceSigner _invoiceSigner;

    public ZatcaInvoiceManager(
        ZatcaInvoiceValidator invoiceValidator,
        ZatcaQrGenerator qrGenerator,
        ZatcaXmlBuilder xmlBuilder,
        ZatcaInvoiceSigner invoiceSigner)
    {
        _invoiceValidator = invoiceValidator;
        _qrGenerator = qrGenerator;
        _xmlBuilder = xmlBuilder;
        _invoiceSigner = invoiceSigner;
    }

    /// <summary>
    /// Prepares an invoice for ZATCA submission by validating, generating QR code,
    /// building XML, signing, and computing hash.
    /// </summary>
    /// <param name="invoice">The invoice entity (with Seller and Lines eagerly loaded)</param>
    /// <param name="certificate">The active ZATCA certificate for signing</param>
    /// <param name="certificatePassword">Password to decrypt the certificate private key</param>
    /// <returns>The updated invoice with QR code, XML content, and hash populated</returns>
    /// <exception cref="BusinessException">Thrown when validation fails or any step encounters an error</exception>
    public async Task<ZatcaInvoice> PrepareInvoiceForSubmissionAsync(
        ZatcaInvoice invoice,
        ZatcaCertificate certificate,
        string certificatePassword = "")
    {
        // Step 1: Validate the invoice
        var validationErrors = _invoiceValidator.Validate(invoice);
        if (validationErrors.Any())
        {
            throw new BusinessException(
                code: "ZATCA:INVOICE_VALIDATION_FAILED",
                message: "Invoice validation failed.",
                details: string.Join(Environment.NewLine, validationErrors));
        }

        // Step 2: Ensure invoice lines amounts are calculated
        foreach (var line in invoice.Lines)
        {
            line.CalculateAmounts();
        }

        // Step 3: Recalculate invoice totals from lines
        RecalculateInvoiceTotals(invoice);

        // Step 4: Generate QR code
        invoice.QrCode = _qrGenerator.GenerateQrCode(
            sellerName: invoice.Seller.SellerNameAr,
            vatRegistrationNumber: invoice.Seller.VatRegistrationNumber,
            invoiceTimestamp: invoice.IssueDate,
            invoiceTotal: invoice.GrandTotal,
            vatTotal: invoice.VatAmount);

        // Step 5: Build the initial XML (unsigned)
        var unsignedXml = _xmlBuilder.BuildInvoiceXml(
            invoice: invoice,
            seller: invoice.Seller,
            invoiceHash: string.Empty, // Placeholder for now
            qrCode: invoice.QrCode);

        // Step 6: Compute the invoice hash from unsigned XML
        invoice.InvoiceHash = _invoiceSigner.ComputeInvoiceHash(unsignedXml);

        // Step 7: Rebuild XML with the computed hash
        unsignedXml = _xmlBuilder.BuildInvoiceXml(
            invoice: invoice,
            seller: invoice.Seller,
            invoiceHash: invoice.InvoiceHash,
            qrCode: invoice.QrCode);

        // Step 8: Sign the XML
        var signedXml = _invoiceSigner.SignInvoice(
            xmlContent: unsignedXml,
            certificate: certificate,
            certificatePassword: certificatePassword);

        // Step 9: Store the signed XML in the invoice
        invoice.XmlContent = signedXml;

        // Step 10: Update invoice status to validated
        invoice.Status = ZatcaInvoiceStatus.Validated;

        return await Task.FromResult(invoice);
    }

    /// <summary>
    /// Validates an invoice without preparing it for submission.
    /// Useful for draft validation before final submission.
    /// </summary>
    /// <param name="invoice">The invoice to validate</param>
    /// <returns>Validation result with errors if any</returns>
    public ZatcaInvoiceValidationResult ValidateInvoice(ZatcaInvoice invoice)
    {
        var errors = _invoiceValidator.Validate(invoice);

        return new ZatcaInvoiceValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }

    /// <summary>
    /// Regenerates the QR code for an existing invoice.
    /// Useful when invoice data changes.
    /// </summary>
    public void RegenerateQrCode(ZatcaInvoice invoice)
    {
        if (invoice.Seller == null)
        {
            throw new BusinessException(
                code: "ZATCA:SELLER_NOT_LOADED",
                message: "Seller must be loaded to regenerate QR code.");
        }

        invoice.QrCode = _qrGenerator.GenerateQrCode(
            sellerName: invoice.Seller.SellerNameAr,
            vatRegistrationNumber: invoice.Seller.VatRegistrationNumber,
            invoiceTimestamp: invoice.IssueDate,
            invoiceTotal: invoice.GrandTotal,
            vatTotal: invoice.VatAmount);
    }

    /// <summary>
    /// Recalculates invoice totals from line items.
    /// </summary>
    private void RecalculateInvoiceTotals(ZatcaInvoice invoice)
    {
        if (!invoice.Lines.Any())
        {
            throw new BusinessException(
                code: "ZATCA:NO_INVOICE_LINES",
                message: "Cannot calculate totals for an invoice with no line items.");
        }

        invoice.SubTotal = invoice.Lines.Sum(l => l.NetAmount);
        invoice.VatAmount = invoice.Lines.Sum(l => l.VatAmount);
        invoice.GrandTotal = invoice.Lines.Sum(l => l.TotalAmount);

        // Round to 2 decimal places
        invoice.SubTotal = Math.Round(invoice.SubTotal, 2);
        invoice.VatAmount = Math.Round(invoice.VatAmount, 2);
        invoice.GrandTotal = Math.Round(invoice.GrandTotal, 2);
    }

    /// <summary>
    /// Links the current invoice to the previous invoice in the chain (for Phase 2).
    /// </summary>
    public void LinkToPreviousInvoice(ZatcaInvoice currentInvoice, ZatcaInvoice previousInvoice)
    {
        if (string.IsNullOrWhiteSpace(previousInvoice.InvoiceHash))
        {
            throw new BusinessException(
                code: "ZATCA:PREVIOUS_INVOICE_NOT_HASHED",
                message: "Previous invoice must be hashed before linking.");
        }

        currentInvoice.PreviousInvoiceHash = previousInvoice.InvoiceHash;
    }
}

/// <summary>
/// Result object for invoice validation.
/// </summary>
public class ZatcaInvoiceValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    public string GetErrorSummary()
    {
        return IsValid
            ? "Validation passed."
            : string.Join(Environment.NewLine, Errors);
    }
}
