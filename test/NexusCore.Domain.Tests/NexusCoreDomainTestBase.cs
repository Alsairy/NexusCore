using Volo.Abp.Modularity;

namespace NexusCore;

/* Inherit from this class for your domain layer tests. */
public abstract class NexusCoreDomainTestBase<TStartupModule> : NexusCoreTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
