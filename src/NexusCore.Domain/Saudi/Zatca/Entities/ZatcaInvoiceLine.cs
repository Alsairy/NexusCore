using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NexusCore.Saudi.Zatca;

public class ZatcaInvoiceLine : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid InvoiceId { get; set; }

    public string ItemName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public string? TaxCategoryCode { get; set; }
    public decimal TaxPercent { get; set; } = ZatcaConsts.DefaultVatRate;

    public decimal NetAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }

    protected ZatcaInvoiceLine() { }

    public ZatcaInvoiceLine(
        Guid id,
        Guid invoiceId,
        string itemName,
        decimal quantity,
        decimal unitPrice,
        Guid? tenantId = null)
        : base(id)
    {
        InvoiceId = invoiceId;
        ItemName = itemName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TenantId = tenantId;
        CalculateAmounts();
    }

    public void CalculateAmounts()
    {
        NetAmount = Math.Round(Quantity * UnitPrice, 2);
        VatAmount = Math.Round(NetAmount * TaxPercent / 100m, 2);
        TotalAmount = Math.Round(NetAmount + VatAmount, 2);
    }
}
