using System.Threading.Tasks;

namespace NexusCore.Data;

public interface INexusCoreDbSchemaMigrator
{
    Task MigrateAsync();
}
