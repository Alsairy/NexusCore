using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NexusCore.Saudi.Nafath;

public class NafathUserLink : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid UserId { get; set; }
    public string NationalId { get; set; } = null!;
    public DateTime VerifiedAt { get; set; }
    public bool IsActive { get; set; }

    protected NafathUserLink() { }

    public NafathUserLink(
        Guid id,
        Guid userId,
        string nationalId,
        DateTime verifiedAt,
        Guid? tenantId = null)
        : base(id)
    {
        UserId = userId;
        NationalId = nationalId;
        VerifiedAt = verifiedAt;
        IsActive = true;
        TenantId = tenantId;
    }
}
