using NexusCore.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace NexusCore.Settings;

public class NexusCoreSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        // Saudi Kit settings
        context.Add(
            new SettingDefinition(
                NexusCoreSettings.Saudi.DefaultCalendar,
                "Hijri",
                L("Setting:Saudi:DefaultCalendar"),
                L("Setting:Saudi:DefaultCalendar:Description"),
                isVisibleToClients: true),

            // ZATCA settings
            new SettingDefinition(
                NexusCoreSettings.Saudi.Zatca.Environment,
                "Sandbox",
                L("Setting:Saudi:Zatca:Environment"),
                L("Setting:Saudi:Zatca:Environment:Description")),

            new SettingDefinition(
                NexusCoreSettings.Saudi.Zatca.ApiBaseUrl,
                "https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal",
                L("Setting:Saudi:Zatca:ApiBaseUrl"),
                L("Setting:Saudi:Zatca:ApiBaseUrl:Description")),

            new SettingDefinition(
                NexusCoreSettings.Saudi.Zatca.ComplianceCsid,
                "",
                L("Setting:Saudi:Zatca:ComplianceCsid"),
                L("Setting:Saudi:Zatca:ComplianceCsid:Description"),
                isEncrypted: true),

            new SettingDefinition(
                NexusCoreSettings.Saudi.Zatca.ProductionCsid,
                "",
                L("Setting:Saudi:Zatca:ProductionCsid"),
                L("Setting:Saudi:Zatca:ProductionCsid:Description"),
                isEncrypted: true),

            new SettingDefinition(
                NexusCoreSettings.Saudi.Zatca.Secret,
                "",
                L("Setting:Saudi:Zatca:Secret"),
                L("Setting:Saudi:Zatca:Secret:Description"),
                isEncrypted: true),

            // Nafath settings
            new SettingDefinition(
                NexusCoreSettings.Saudi.Nafath.AppId,
                "",
                L("Setting:Saudi:Nafath:AppId"),
                L("Setting:Saudi:Nafath:AppId:Description")),

            new SettingDefinition(
                NexusCoreSettings.Saudi.Nafath.AppKey,
                "",
                L("Setting:Saudi:Nafath:AppKey"),
                L("Setting:Saudi:Nafath:AppKey:Description"),
                isEncrypted: true),

            new SettingDefinition(
                NexusCoreSettings.Saudi.Nafath.ApiBaseUrl,
                "https://nafath.api.elm.sa",
                L("Setting:Saudi:Nafath:ApiBaseUrl"),
                L("Setting:Saudi:Nafath:ApiBaseUrl:Description")),

            new SettingDefinition(
                NexusCoreSettings.Saudi.Nafath.CallbackUrl,
                "",
                L("Setting:Saudi:Nafath:CallbackUrl"),
                L("Setting:Saudi:Nafath:CallbackUrl:Description")),

            new SettingDefinition(
                NexusCoreSettings.Saudi.Nafath.TimeoutSeconds,
                "120",
                L("Setting:Saudi:Nafath:TimeoutSeconds"),
                L("Setting:Saudi:Nafath:TimeoutSeconds:Description"))
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<NexusCoreResource>(name);
    }
}
