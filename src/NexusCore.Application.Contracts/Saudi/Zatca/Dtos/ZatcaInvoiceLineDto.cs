using System;
using System.ComponentModel.DataAnnotations;

namespace NexusCore.Saudi.Zatca;

/// <summary>
/// Full DTO for ZATCA Invoice Line entity
/// </summary>
public class ZatcaInvoiceLineDto
{
    public Guid Id { get; set; }

    public Guid InvoiceId { get; set; }

    public string ItemName { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public string? TaxCategoryCode { get; set; }

    public decimal TaxPercent { get; set; }

    public decimal NetAmount { get; set; }

    public decimal VatAmount { get; set; }

    public decimal TotalAmount { get; set; }
}

/// <summary>
/// DTO for creating or updating a ZATCA Invoice Line
/// </summary>
public class CreateUpdateZatcaInvoiceLineDto
{
    [Required]
    [StringLength(ZatcaConsts.MaxItemNameLength)]
    public string ItemName { get; set; } = string.Empty;

    [Required]
    [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Unit price must be non-negative")]
    public decimal UnitPrice { get; set; }

    [StringLength(ZatcaConsts.MaxTaxCategoryCodeLength)]
    public string? TaxCategoryCode { get; set; }

    [Range(0, 100, ErrorMessage = "Tax percent must be between 0 and 100")]
    public decimal TaxPercent { get; set; } = ZatcaConsts.DefaultVatRate;
}
