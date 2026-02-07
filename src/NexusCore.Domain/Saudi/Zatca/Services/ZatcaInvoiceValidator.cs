using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Volo.Abp.DependencyInjection;

namespace NexusCore.Saudi.Zatca;

/// <summary>
/// Validates ZATCA invoice entities before submission to ensure compliance.
/// </summary>
public class ZatcaInvoiceValidator : ITransientDependency
{
    private static readonly Regex VatNumberRegex = new Regex(@"^3\d{14}$", RegexOptions.Compiled);

    /// <summary>
    /// Validates a ZATCA invoice entity and returns a list of validation errors.
    /// </summary>
    /// <param name="invoice">The invoice to validate (with Seller and Lines loaded)</param>
    /// <returns>List of validation error messages. Empty if valid.</returns>
    public List<string> Validate(ZatcaInvoice invoice)
    {
        var errors = new List<string>();

        // Basic invoice validations
        ValidateBasicInvoiceData(invoice, errors);

        // Seller validations
        ValidateSeller(invoice, errors);

        // Buyer validations (for standard invoices)
        ValidateBuyer(invoice, errors);

        // Invoice lines validations
        ValidateInvoiceLines(invoice, errors);

        // Amounts validations
        ValidateAmounts(invoice, errors);

        // Currency validations
        ValidateCurrency(invoice, errors);

        // Date validations
        ValidateDates(invoice, errors);

        return errors;
    }

    private void ValidateBasicInvoiceData(ZatcaInvoice invoice, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
        {
            errors.Add("Invoice number is required.");
        }
        else if (invoice.InvoiceNumber.Length > ZatcaConsts.MaxInvoiceNumberLength)
        {
            errors.Add($"Invoice number cannot exceed {ZatcaConsts.MaxInvoiceNumberLength} characters.");
        }

        if (invoice.Id == Guid.Empty)
        {
            errors.Add("Invoice ID (UUID) is required.");
        }

        if (invoice.SellerId == Guid.Empty)
        {
            errors.Add("Seller ID is required.");
        }

        if (!Enum.IsDefined(typeof(ZatcaInvoiceType), invoice.InvoiceType))
        {
            errors.Add("Invalid invoice type.");
        }
    }

    private void ValidateSeller(ZatcaInvoice invoice, List<string> errors)
    {
        if (invoice.Seller == null)
        {
            errors.Add("Seller information is required. Ensure Seller navigation property is loaded.");
            return;
        }

        var seller = invoice.Seller;

        if (string.IsNullOrWhiteSpace(seller.SellerNameAr))
        {
            errors.Add("Seller Arabic name is required.");
        }

        if (string.IsNullOrWhiteSpace(seller.VatRegistrationNumber))
        {
            errors.Add("Seller VAT registration number is required.");
        }
        else if (!VatNumberRegex.IsMatch(seller.VatRegistrationNumber))
        {
            errors.Add("Seller VAT registration number must be 15 digits starting with 3.");
        }

        // Address validations for standard invoices
        if (invoice.InvoiceType == ZatcaInvoiceType.Standard ||
            invoice.InvoiceType == ZatcaInvoiceType.StandardCreditNote ||
            invoice.InvoiceType == ZatcaInvoiceType.StandardDebitNote)
        {
            if (string.IsNullOrWhiteSpace(seller.Street))
            {
                errors.Add("Seller street address is required for standard invoices.");
            }

            if (string.IsNullOrWhiteSpace(seller.City))
            {
                errors.Add("Seller city is required for standard invoices.");
            }

            if (string.IsNullOrWhiteSpace(seller.PostalCode))
            {
                errors.Add("Seller postal code is required for standard invoices.");
            }
        }
    }

    private void ValidateBuyer(ZatcaInvoice invoice, List<string> errors)
    {
        // For standard invoices (B2B), buyer information is required
        var isStandardInvoice = invoice.InvoiceType == ZatcaInvoiceType.Standard ||
                                invoice.InvoiceType == ZatcaInvoiceType.StandardCreditNote ||
                                invoice.InvoiceType == ZatcaInvoiceType.StandardDebitNote;

        if (isStandardInvoice)
        {
            if (string.IsNullOrWhiteSpace(invoice.BuyerName))
            {
                errors.Add("Buyer name is required for standard invoices.");
            }

            if (string.IsNullOrWhiteSpace(invoice.BuyerVatNumber))
            {
                errors.Add("Buyer VAT number is required for standard invoices.");
            }
            else if (!VatNumberRegex.IsMatch(invoice.BuyerVatNumber))
            {
                errors.Add("Buyer VAT registration number must be 15 digits starting with 3.");
            }
        }
    }

    private void ValidateInvoiceLines(ZatcaInvoice invoice, List<string> errors)
    {
        if (invoice.Lines == null || !invoice.Lines.Any())
        {
            errors.Add("Invoice must have at least one line item.");
            return;
        }

        var lineNumber = 1;
        foreach (var line in invoice.Lines)
        {
            var linePrefix = $"Line {lineNumber}: ";

            if (string.IsNullOrWhiteSpace(line.ItemName))
            {
                errors.Add($"{linePrefix}Item name is required.");
            }
            else if (line.ItemName.Length > ZatcaConsts.MaxItemNameLength)
            {
                errors.Add($"{linePrefix}Item name cannot exceed {ZatcaConsts.MaxItemNameLength} characters.");
            }

            if (line.Quantity <= 0)
            {
                errors.Add($"{linePrefix}Quantity must be greater than zero.");
            }

            if (line.UnitPrice < 0)
            {
                errors.Add($"{linePrefix}Unit price cannot be negative.");
            }

            if (line.TaxPercent < 0 || line.TaxPercent > 100)
            {
                errors.Add($"{linePrefix}Tax percentage must be between 0 and 100.");
            }

            if (line.NetAmount < 0)
            {
                errors.Add($"{linePrefix}Net amount cannot be negative.");
            }

            if (line.VatAmount < 0)
            {
                errors.Add($"{linePrefix}VAT amount cannot be negative.");
            }

            if (line.TotalAmount < 0)
            {
                errors.Add($"{linePrefix}Total amount cannot be negative.");
            }

            // Verify calculation consistency
            var expectedNetAmount = Math.Round(line.Quantity * line.UnitPrice, 2);
            if (Math.Abs(line.NetAmount - expectedNetAmount) > 0.01m)
            {
                errors.Add($"{linePrefix}Net amount calculation is incorrect. Expected {expectedNetAmount:F2}, got {line.NetAmount:F2}.");
            }

            lineNumber++;
        }
    }

    private void ValidateAmounts(ZatcaInvoice invoice, List<string> errors)
    {
        if (invoice.SubTotal < 0)
        {
            errors.Add("Invoice subtotal cannot be negative.");
        }

        if (invoice.VatAmount < 0)
        {
            errors.Add("Invoice VAT amount cannot be negative.");
        }

        if (invoice.GrandTotal <= 0)
        {
            errors.Add("Invoice grand total must be greater than zero.");
        }

        // Verify amounts match line totals
        if (invoice.Lines != null && invoice.Lines.Any())
        {
            var calculatedSubTotal = invoice.Lines.Sum(l => l.NetAmount);
            var calculatedVatTotal = invoice.Lines.Sum(l => l.VatAmount);
            var calculatedGrandTotal = invoice.Lines.Sum(l => l.TotalAmount);

            if (Math.Abs(invoice.SubTotal - calculatedSubTotal) > 0.01m)
            {
                errors.Add($"Invoice subtotal ({invoice.SubTotal:F2}) does not match sum of line net amounts ({calculatedSubTotal:F2}).");
            }

            if (Math.Abs(invoice.VatAmount - calculatedVatTotal) > 0.01m)
            {
                errors.Add($"Invoice VAT amount ({invoice.VatAmount:F2}) does not match sum of line VAT amounts ({calculatedVatTotal:F2}).");
            }

            if (Math.Abs(invoice.GrandTotal - calculatedGrandTotal) > 0.01m)
            {
                errors.Add($"Invoice grand total ({invoice.GrandTotal:F2}) does not match sum of line totals ({calculatedGrandTotal:F2}).");
            }
        }

        // Verify grand total = subtotal + VAT
        var expectedGrandTotal = invoice.SubTotal + invoice.VatAmount;
        if (Math.Abs(invoice.GrandTotal - expectedGrandTotal) > 0.01m)
        {
            errors.Add($"Invoice grand total ({invoice.GrandTotal:F2}) should equal subtotal + VAT ({expectedGrandTotal:F2}).");
        }
    }

    private void ValidateCurrency(ZatcaInvoice invoice, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(invoice.CurrencyCode))
        {
            errors.Add("Currency code is required.");
        }
        else if (invoice.CurrencyCode.Length != 3)
        {
            errors.Add("Currency code must be a 3-letter ISO code (e.g., SAR, USD).");
        }
        else if (invoice.CurrencyCode != ZatcaConsts.DefaultCurrencyCode)
        {
            // ZATCA primarily supports SAR, but may support other currencies
            // This is a warning rather than a hard error
            errors.Add($"Warning: Currency code is {invoice.CurrencyCode}. ZATCA primarily supports {ZatcaConsts.DefaultCurrencyCode}.");
        }
    }

    private void ValidateDates(ZatcaInvoice invoice, List<string> errors)
    {
        if (invoice.IssueDate == default)
        {
            errors.Add("Invoice issue date is required.");
        }

        if (invoice.IssueDate > DateTime.UtcNow.AddDays(1))
        {
            errors.Add("Invoice issue date cannot be in the future.");
        }

        // ZATCA requires invoices to be submitted within a reasonable timeframe
        var maxDaysOld = 365; // Configurable based on ZATCA rules
        if (invoice.IssueDate < DateTime.UtcNow.AddDays(-maxDaysOld))
        {
            errors.Add($"Invoice issue date cannot be older than {maxDaysOld} days.");
        }
    }

    /// <summary>
    /// Quick validation to check if the invoice has basic required fields.
    /// </summary>
    public bool IsValid(ZatcaInvoice invoice)
    {
        var errors = Validate(invoice);
        return errors.Count == 0;
    }
}
