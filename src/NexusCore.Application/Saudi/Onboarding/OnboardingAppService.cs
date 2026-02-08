using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NexusCore.Permissions;
using Volo.Abp.Domain.Repositories;

namespace NexusCore.Saudi.Onboarding;

[Authorize]
public class OnboardingAppService : NexusCoreAppService, IOnboardingAppService
{
    private readonly IRepository<TenantOnboardingStatus, Guid> _onboardingRepository;

    public OnboardingAppService(IRepository<TenantOnboardingStatus, Guid> onboardingRepository)
    {
        _onboardingRepository = onboardingRepository;
    }

    public async Task<OnboardingStatusDto> GetStatusAsync()
    {
        var status = await GetOrCreateStatusAsync();
        return MapToDto(status);
    }

    public async Task<OnboardingStatusDto> CompleteStepAsync(OnboardingStep step)
    {
        var status = await GetOrCreateStatusAsync();

        switch (step)
        {
            case OnboardingStep.ZatcaConfigured:
                status.ZatcaConfigured = true;
                break;
            case OnboardingStep.SellerCreated:
                status.SellerCreated = true;
                break;
            case OnboardingStep.CertificateUploaded:
                status.CertificateUploaded = true;
                break;
            case OnboardingStep.FirstInvoiceSubmitted:
                status.FirstInvoiceSubmitted = true;
                break;
            case OnboardingStep.NafathConfigured:
                status.NafathConfigured = true;
                break;
        }

        status.MarkCompleteIfReady();
        await _onboardingRepository.UpdateAsync(status);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return MapToDto(status);
    }

    [Authorize(NexusCorePermissions.Zatca.ManageSettings)]
    public async Task ResetAsync()
    {
        var status = await _onboardingRepository.FirstOrDefaultAsync();
        if (status != null)
        {
            status.ZatcaConfigured = false;
            status.SellerCreated = false;
            status.CertificateUploaded = false;
            status.FirstInvoiceSubmitted = false;
            status.NafathConfigured = false;
            status.CompletedAt = null;

            await _onboardingRepository.UpdateAsync(status);
            await CurrentUnitOfWork!.SaveChangesAsync();
        }
    }

    private async Task<TenantOnboardingStatus> GetOrCreateStatusAsync()
    {
        var status = await _onboardingRepository.FirstOrDefaultAsync();
        if (status == null)
        {
            status = new TenantOnboardingStatus(GuidGenerator.Create(), CurrentTenant.Id);
            await _onboardingRepository.InsertAsync(status);
            await CurrentUnitOfWork!.SaveChangesAsync();
        }

        return status;
    }

    private static OnboardingStatusDto MapToDto(TenantOnboardingStatus status)
    {
        return new OnboardingStatusDto
        {
            Id = status.Id,
            ZatcaConfigured = status.ZatcaConfigured,
            SellerCreated = status.SellerCreated,
            CertificateUploaded = status.CertificateUploaded,
            FirstInvoiceSubmitted = status.FirstInvoiceSubmitted,
            NafathConfigured = status.NafathConfigured,
            IsComplete = status.IsComplete,
            CompletedAt = status.CompletedAt,
            CompletedSteps = status.CompletedSteps,
            TotalRequiredSteps = status.TotalRequiredSteps,
            TotalSteps = status.TotalSteps
        };
    }
}
