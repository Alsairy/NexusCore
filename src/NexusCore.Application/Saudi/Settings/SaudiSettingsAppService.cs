using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NexusCore.Permissions;
using NexusCore.Settings;
using Volo.Abp.SettingManagement;

namespace NexusCore.Saudi.Settings;

public class SaudiSettingsAppService : NexusCoreAppService, ISaudiSettingsAppService
{
    private readonly ISettingManager _settingManager;

    public SaudiSettingsAppService(ISettingManager settingManager)
    {
        _settingManager = settingManager;
    }

    [Authorize(NexusCorePermissions.Zatca.ManageSettings)]
    public async Task<ZatcaSettingsDto> GetZatcaSettingsAsync()
    {
        return new ZatcaSettingsDto
        {
            Environment = await _settingManager.GetOrNullForCurrentTenantAsync(NexusCoreSettings.Saudi.Zatca.Environment) ?? "Sandbox",
            ApiBaseUrl = await _settingManager.GetOrNullForCurrentTenantAsync(NexusCoreSettings.Saudi.Zatca.ApiBaseUrl) ?? string.Empty,
            ComplianceCsid = await _settingManager.GetOrNullForCurrentTenantAsync(NexusCoreSettings.Saudi.Zatca.ComplianceCsid),
            ProductionCsid = await _settingManager.GetOrNullForCurrentTenantAsync(NexusCoreSettings.Saudi.Zatca.ProductionCsid),
            Secret = await _settingManager.GetOrNullForCurrentTenantAsync(NexusCoreSettings.Saudi.Zatca.Secret)
        };
    }

    [Authorize(NexusCorePermissions.Zatca.ManageSettings)]
    public async Task UpdateZatcaSettingsAsync(ZatcaSettingsDto input)
    {
        await _settingManager.SetForCurrentTenantAsync(NexusCoreSettings.Saudi.Zatca.Environment, input.Environment);
        await _settingManager.SetForCurrentTenantAsync(NexusCoreSettings.Saudi.Zatca.ApiBaseUrl, input.ApiBaseUrl);
        await _settingManager.SetForCurrentTenantAsync(NexusCoreSettings.Saudi.Zatca.ComplianceCsid, input.ComplianceCsid ?? string.Empty);
        await _settingManager.SetForCurrentTenantAsync(NexusCoreSettings.Saudi.Zatca.ProductionCsid, input.ProductionCsid ?? string.Empty);
        await _settingManager.SetForCurrentTenantAsync(NexusCoreSettings.Saudi.Zatca.Secret, input.Secret ?? string.Empty);
    }

    [Authorize(NexusCorePermissions.Nafath.ManageSettings)]
    public async Task<NafathSettingsDto> GetNafathSettingsAsync()
    {
        var timeoutStr = await _settingManager.GetOrNullForCurrentTenantAsync(NexusCoreSettings.Saudi.Nafath.TimeoutSeconds);
        _ = int.TryParse(timeoutStr, out var timeoutSeconds);
        if (timeoutSeconds < 30) timeoutSeconds = 120;

        return new NafathSettingsDto
        {
            AppId = await _settingManager.GetOrNullForCurrentTenantAsync(NexusCoreSettings.Saudi.Nafath.AppId),
            AppKey = await _settingManager.GetOrNullForCurrentTenantAsync(NexusCoreSettings.Saudi.Nafath.AppKey),
            ApiBaseUrl = await _settingManager.GetOrNullForCurrentTenantAsync(NexusCoreSettings.Saudi.Nafath.ApiBaseUrl) ?? string.Empty,
            CallbackUrl = await _settingManager.GetOrNullForCurrentTenantAsync(NexusCoreSettings.Saudi.Nafath.CallbackUrl),
            TimeoutSeconds = timeoutSeconds
        };
    }

    [Authorize(NexusCorePermissions.Nafath.ManageSettings)]
    public async Task UpdateNafathSettingsAsync(NafathSettingsDto input)
    {
        await _settingManager.SetForCurrentTenantAsync(NexusCoreSettings.Saudi.Nafath.AppId, input.AppId ?? string.Empty);
        await _settingManager.SetForCurrentTenantAsync(NexusCoreSettings.Saudi.Nafath.AppKey, input.AppKey ?? string.Empty);
        await _settingManager.SetForCurrentTenantAsync(NexusCoreSettings.Saudi.Nafath.ApiBaseUrl, input.ApiBaseUrl);
        await _settingManager.SetForCurrentTenantAsync(NexusCoreSettings.Saudi.Nafath.CallbackUrl, input.CallbackUrl ?? string.Empty);
        await _settingManager.SetForCurrentTenantAsync(NexusCoreSettings.Saudi.Nafath.TimeoutSeconds, input.TimeoutSeconds.ToString());
    }
}
