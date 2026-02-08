using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NexusCore.Saudi.Onboarding;

public interface IOnboardingAppService : IApplicationService
{
    Task<OnboardingStatusDto> GetStatusAsync();
    Task<OnboardingStatusDto> CompleteStepAsync(OnboardingStep step);
    Task ResetAsync();
}
