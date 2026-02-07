using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace NexusCore.Data;

/* This is used if database provider does't define
 * INexusCoreDbSchemaMigrator implementation.
 */
public class NullNexusCoreDbSchemaMigrator : INexusCoreDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
