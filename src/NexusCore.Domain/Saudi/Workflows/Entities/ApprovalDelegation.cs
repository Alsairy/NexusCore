using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NexusCore.Saudi.Workflows;

public class ApprovalDelegation : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid DelegatorUserId { get; set; }
    public Guid DelegateUserId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? Reason { get; set; }

    protected ApprovalDelegation() { }

    public ApprovalDelegation(
        Guid id,
        Guid delegatorUserId,
        Guid delegateUserId,
        DateTime startDate,
        DateTime endDate,
        string? reason = null,
        Guid? tenantId = null)
        : base(id)
    {
        DelegatorUserId = delegatorUserId;
        DelegateUserId = delegateUserId;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = true;
        Reason = reason;
        TenantId = tenantId;
    }
}
