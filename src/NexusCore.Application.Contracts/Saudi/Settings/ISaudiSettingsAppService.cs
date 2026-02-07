using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NexusCore.Saudi.Settings;

public interface ISaudiSettingsAppService : IApplicationService
{
    Task<ZatcaSettingsDto> GetZatcaSettingsAsync();

    Task UpdateZatcaSettingsAsync(ZatcaSettingsDto input);

    Task<NafathSettingsDto> GetNafathSettingsAsync();

    Task UpdateNafathSettingsAsync(NafathSettingsDto input);
}
