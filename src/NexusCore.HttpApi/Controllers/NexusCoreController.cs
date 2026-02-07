using NexusCore.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace NexusCore.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class NexusCoreController : AbpControllerBase
{
    protected NexusCoreController()
    {
        LocalizationResource = typeof(NexusCoreResource);
    }
}
