using System;
using Volo.Abp.Application.Dtos;

namespace NexusCore.Saudi.Zatca;

/// <summary>
/// Input DTO for getting paginated and filtered list of ZATCA invoices
/// </summary>
public class GetZatcaInvoiceListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// Filter by invoice status
    /// </summary>
    public ZatcaInvoiceStatus? Status { get; set; }

    /// <summary>
    /// Filter by invoice type
    /// </summary>
    public ZatcaInvoiceType? InvoiceType { get; set; }

    /// <summary>
    /// Filter by invoice date from (inclusive)
    /// </summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// Filter by invoice date to (inclusive)
    /// </summary>
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// Search filter for invoice number or customer name
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// Filter by seller ID
    /// </summary>
    public Guid? SellerId { get; set; }
}
