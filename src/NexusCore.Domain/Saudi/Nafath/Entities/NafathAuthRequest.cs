using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NexusCore.Saudi.Nafath;

public class NafathAuthRequest : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string TransactionId { get; set; } = null!;
    public string NationalId { get; set; } = null!;
    public int RandomNumber { get; set; }
    public NafathRequestStatus Status { get; set; }

    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public Guid? UserId { get; set; }

    protected NafathAuthRequest() { }

    public NafathAuthRequest(
        Guid id,
        string transactionId,
        string nationalId,
        int randomNumber,
        DateTime requestedAt,
        DateTime expiresAt,
        Guid? tenantId = null)
        : base(id)
    {
        TransactionId = transactionId;
        NationalId = nationalId;
        RandomNumber = randomNumber;
        Status = NafathRequestStatus.Pending;
        RequestedAt = requestedAt;
        ExpiresAt = expiresAt;
        TenantId = tenantId;
    }

    public void MarkCompleted(Guid? userId = null)
    {
        Status = NafathRequestStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UserId = userId;
    }

    public void MarkRejected()
    {
        Status = NafathRequestStatus.Rejected;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkExpired()
    {
        Status = NafathRequestStatus.Expired;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        Status = NafathRequestStatus.Failed;
        CompletedAt = DateTime.UtcNow;
    }
}
