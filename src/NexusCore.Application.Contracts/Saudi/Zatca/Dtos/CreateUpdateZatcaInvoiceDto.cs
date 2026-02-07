using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NexusCore.Saudi.Zatca;

/// <summary>
/// DTO for creating or updating a ZATCA Invoice with lines
/// </summary>
public class CreateUpdateZatcaInvoiceDto
{
    [Required]
    public Guid SellerId { get; set; }

    [Required]
    [StringLength(ZatcaConsts.MaxInvoiceNumberLength)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    public ZatcaInvoiceType InvoiceType { get; set; }

    [Required]
    public DateTime IssueDate { get; set; }

    [StringLength(20)]
    public string? IssueDateHijri { get; set; }

    [StringLength(ZatcaConsts.MaxBuyerNameLength)]
    public string? BuyerName { get; set; }

    [StringLength(ZatcaConsts.MaxVatRegistrationNumberLength)]
    [RegularExpression(@"^\d{15}$", ErrorMessage = "Buyer VAT Number must be exactly 15 digits")]
    public string? BuyerVatNumber { get; set; }

    [StringLength(ZatcaConsts.MaxCurrencyCodeLength)]
    public string CurrencyCode { get; set; } = ZatcaConsts.DefaultCurrencyCode;

    [Required]
    [MinLength(1, ErrorMessage = "At least one invoice line is required")]
    public List<CreateUpdateZatcaInvoiceLineDto> Lines { get; set; } = new();
}
