using Microsoft.Extensions.Localization;
using NexusCore.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace NexusCore;

[Dependency(ReplaceServices = true)]
public class NexusCoreBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<NexusCoreResource> _localizer;

    public NexusCoreBrandingProvider(IStringLocalizer<NexusCoreResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
