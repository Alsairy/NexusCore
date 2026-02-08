using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NexusCore.Saudi.Onboarding;

public class TenantOnboardingStatus : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>Step 1: ZATCA API settings configured</summary>
    public bool ZatcaConfigured { get; set; }

    /// <summary>Step 2: At least one seller created</summary>
    public bool SellerCreated { get; set; }

    /// <summary>Step 3: Certificate uploaded for a seller</summary>
    public bool CertificateUploaded { get; set; }

    /// <summary>Step 4: First invoice submitted to ZATCA</summary>
    public bool FirstInvoiceSubmitted { get; set; }

    /// <summary>Step 5 (optional): Nafath SSO configured</summary>
    public bool NafathConfigured { get; set; }

    /// <summary>Timestamp when all required steps were completed</summary>
    public DateTime? CompletedAt { get; set; }

    protected TenantOnboardingStatus() { }

    public TenantOnboardingStatus(Guid id, Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
    }

    public bool IsComplete =>
        ZatcaConfigured && SellerCreated && CertificateUploaded && FirstInvoiceSubmitted;

    public void MarkCompleteIfReady()
    {
        if (IsComplete && CompletedAt == null)
        {
            CompletedAt = DateTime.UtcNow;
        }
    }

    public int CompletedSteps
    {
        get
        {
            var count = 0;
            if (ZatcaConfigured) count++;
            if (SellerCreated) count++;
            if (CertificateUploaded) count++;
            if (FirstInvoiceSubmitted) count++;
            if (NafathConfigured) count++;
            return count;
        }
    }

    public int TotalRequiredSteps => 4;
    public int TotalSteps => 5;
}
