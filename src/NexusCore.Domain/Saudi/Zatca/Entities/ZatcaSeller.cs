using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NexusCore.Saudi.Zatca;

public class ZatcaSeller : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string SellerNameAr { get; set; } = null!;
    public string? SellerNameEn { get; set; }

    public string VatRegistrationNumber { get; set; } = null!;
    public string? CommercialRegistrationNumber { get; set; }

    public string? Street { get; set; }
    public string? BuildingNumber { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }

    public bool IsDefault { get; set; }

    protected ZatcaSeller() { }

    public ZatcaSeller(
        Guid id,
        string sellerNameAr,
        string vatRegistrationNumber,
        Guid? tenantId = null)
        : base(id)
    {
        SellerNameAr = sellerNameAr;
        VatRegistrationNumber = vatRegistrationNumber;
        TenantId = tenantId;
    }
}
