using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NexusCore.Saudi.Zatca;

public class ZatcaInvoice : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid SellerId { get; set; }
    public virtual ZatcaSeller Seller { get; set; } = null!;

    public string InvoiceNumber { get; set; } = null!;
    public ZatcaInvoiceType InvoiceType { get; set; }
    public ZatcaInvoiceStatus Status { get; set; }

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

    // ZATCA response fields
    public string? ZatcaRequestId { get; set; }
    public string? ZatcaWarnings { get; set; }
    public string? ZatcaErrors { get; set; }

    public virtual ICollection<ZatcaInvoiceLine> Lines { get; set; } = new List<ZatcaInvoiceLine>();

    protected ZatcaInvoice() { }

    public ZatcaInvoice(
        Guid id,
        Guid sellerId,
        string invoiceNumber,
        ZatcaInvoiceType invoiceType,
        DateTime issueDate,
        Guid? tenantId = null)
        : base(id)
    {
        SellerId = sellerId;
        InvoiceNumber = invoiceNumber;
        InvoiceType = invoiceType;
        IssueDate = issueDate;
        Status = ZatcaInvoiceStatus.Draft;
        TenantId = tenantId;
    }
}
