using System;

namespace NexusCore.Saudi.Onboarding;

public class OnboardingStatusDto
{
    public Guid Id { get; set; }
    public bool ZatcaConfigured { get; set; }
    public bool SellerCreated { get; set; }
    public bool CertificateUploaded { get; set; }
    public bool FirstInvoiceSubmitted { get; set; }
    public bool NafathConfigured { get; set; }
    public bool IsComplete { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int CompletedSteps { get; set; }
    public int TotalRequiredSteps { get; set; }
    public int TotalSteps { get; set; }
}

public enum OnboardingStep
{
    ZatcaConfigured = 1,
    SellerCreated = 2,
    CertificateUploaded = 3,
    FirstInvoiceSubmitted = 4,
    NafathConfigured = 5
}
