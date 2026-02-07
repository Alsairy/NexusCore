using System;
using System.Collections.Generic;

namespace NexusCore.Saudi.Zatca;

/// <summary>
/// Full DTO for ZATCA Invoice entity with all details
/// </summary>
public class ZatcaInvoiceDto
{
    public Guid Id { get; set; }

    public Guid SellerId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public ZatcaInvoiceType InvoiceType { get; set; }

    public DateTime IssueDate { get; set; }

    public string? IssueDateHijri { get; set; }

    public string? BuyerName { get; set; }

    public string? BuyerVatNumber { get; set; }

    public string CurrencyCode { get; set; } = ZatcaConsts.DefaultCurrencyCode;

    public decimal SubTotal { get; set; }

    public decimal VatAmount { get; set; }

    public decimal GrandTotal { get; set; }

    public string? QrCode { get; set; }

    public string? XmlContent { get; set; }

    public string? InvoiceHash { get; set; }

    public string? PreviousInvoiceHash { get; set; }

    public ZatcaInvoiceStatus Status { get; set; }

    public string? ZatcaRequestId { get; set; }

    public string? ZatcaWarnings { get; set; }

    public string? ZatcaErrors { get; set; }

    public DateTime CreationTime { get; set; }

    public List<ZatcaInvoiceLineDto> Lines { get; set; } = new();
}

/// <summary>
/// Lightweight DTO for ZATCA Invoice list view (no XML/QR/Lines)
/// </summary>
public class ZatcaInvoiceListDto
{
    public Guid Id { get; set; }

    public Guid SellerId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public ZatcaInvoiceType InvoiceType { get; set; }

    public DateTime IssueDate { get; set; }

    public string? BuyerName { get; set; }

    public string? BuyerVatNumber { get; set; }

    public decimal SubTotal { get; set; }

    public decimal VatAmount { get; set; }

    public decimal GrandTotal { get; set; }

    public ZatcaInvoiceStatus Status { get; set; }

    public string? ZatcaRequestId { get; set; }

    public DateTime CreationTime { get; set; }
}
