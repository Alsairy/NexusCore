using NexusCore.Localization;
using Volo.Abp.Application.Services;

namespace NexusCore;

/* Inherit your application services from this class.
 */
public abstract class NexusCoreAppService : ApplicationService
{
    protected NexusCoreAppService()
    {
        LocalizationResource = typeof(NexusCoreResource);
    }
}
