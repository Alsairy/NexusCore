using Volo.Abp.Modularity;

namespace NexusCore;

[DependsOn(
    typeof(NexusCoreApplicationModule),
    typeof(NexusCoreDomainTestModule)
)]
public class NexusCoreApplicationTestModule : AbpModule
{

}
