using Volo.Abp.Modularity;

namespace NexusCore;

public abstract class NexusCoreApplicationTestBase<TStartupModule> : NexusCoreTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
