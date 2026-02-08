using System.Threading.Tasks;
using NexusCore.EntityFrameworkCore;
using NexusCore.Saudi.Settings;
using Shouldly;
using Xunit;

namespace NexusCore.Saudi.Tests.Settings;

[Collection(NexusCoreTestConsts.CollectionDefinitionName)]
public class SaudiSettingsAppServiceTests : NexusCoreEntityFrameworkCoreTestBase
{
    private readonly ISaudiSettingsAppService _settingsAppService;

    public SaudiSettingsAppServiceTests()
    {
        _settingsAppService = GetRequiredService<ISaudiSettingsAppService>();
    }

    [Fact]
    public async Task GetZatcaSettings_Should_Return_Defaults()
    {
        var result = await _settingsAppService.GetZatcaSettingsAsync();

        result.ShouldNotBeNull();
        result.Environment.ShouldBe("Sandbox");
    }

    [Fact]
    public async Task UpdateZatcaSettings_Should_Persist()
    {
        var input = new ZatcaSettingsDto
        {
            Environment = "Production",
            ApiBaseUrl = "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal",
            ComplianceCsid = "test-compliance-csid",
            ProductionCsid = "test-production-csid",
            Secret = "test-secret-value"
        };

        await _settingsAppService.UpdateZatcaSettingsAsync(input);

        var result = await _settingsAppService.GetZatcaSettingsAsync();
        result.Environment.ShouldBe("Production");
        result.ApiBaseUrl.ShouldBe("https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal");
        result.ComplianceCsid.ShouldBe("test-compliance-csid");
        result.ProductionCsid.ShouldBe("test-production-csid");
        result.Secret.ShouldBe("test-secret-value");
    }

    [Fact]
    public async Task GetNafathSettings_Should_Return_Defaults()
    {
        var result = await _settingsAppService.GetNafathSettingsAsync();

        result.ShouldNotBeNull();
        result.TimeoutSeconds.ShouldBeGreaterThanOrEqualTo(30);
    }

    [Fact]
    public async Task UpdateNafathSettings_Should_Persist()
    {
        var input = new NafathSettingsDto
        {
            AppId = "nafath-app-001",
            AppKey = "nafath-key-secret",
            ApiBaseUrl = "https://nafath.api.elm.sa",
            CallbackUrl = "https://myapp.com/api/nafath/callback",
            TimeoutSeconds = 180
        };

        await _settingsAppService.UpdateNafathSettingsAsync(input);

        var result = await _settingsAppService.GetNafathSettingsAsync();
        result.AppId.ShouldBe("nafath-app-001");
        result.AppKey.ShouldBe("nafath-key-secret");
        result.ApiBaseUrl.ShouldBe("https://nafath.api.elm.sa");
        result.CallbackUrl.ShouldBe("https://myapp.com/api/nafath/callback");
        result.TimeoutSeconds.ShouldBe(180);
    }

    [Fact]
    public async Task UpdateZatcaSettings_With_Null_Optional_Fields_Should_Persist_Empty()
    {
        var input = new ZatcaSettingsDto
        {
            Environment = "Sandbox",
            ApiBaseUrl = "https://sandbox.zatca.gov.sa",
            ComplianceCsid = null,
            ProductionCsid = null,
            Secret = null
        };

        await _settingsAppService.UpdateZatcaSettingsAsync(input);

        var result = await _settingsAppService.GetZatcaSettingsAsync();
        result.Environment.ShouldBe("Sandbox");
        result.ApiBaseUrl.ShouldBe("https://sandbox.zatca.gov.sa");
    }
}
