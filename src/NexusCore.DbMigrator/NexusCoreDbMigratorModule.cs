using NexusCore.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace NexusCore.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(NexusCoreEntityFrameworkCoreModule),
    typeof(NexusCoreApplicationContractsModule)
)]
public class NexusCoreDbMigratorModule : AbpModule
{
}
