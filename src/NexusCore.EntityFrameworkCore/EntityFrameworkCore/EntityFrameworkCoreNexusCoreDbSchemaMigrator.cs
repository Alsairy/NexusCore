using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexusCore.Data;
using Volo.Abp.DependencyInjection;

namespace NexusCore.EntityFrameworkCore;

public class EntityFrameworkCoreNexusCoreDbSchemaMigrator
    : INexusCoreDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreNexusCoreDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the NexusCoreDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<NexusCoreDbContext>()
            .Database
            .MigrateAsync();
    }
}
