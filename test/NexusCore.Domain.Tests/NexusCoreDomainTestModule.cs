using Volo.Abp.Modularity;

namespace NexusCore;

[DependsOn(
    typeof(NexusCoreDomainModule),
    typeof(NexusCoreTestBaseModule)
)]
public class NexusCoreDomainTestModule : AbpModule
{

}
