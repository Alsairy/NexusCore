using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace NexusCore.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class NexusCoreDbContextFactory : IDesignTimeDbContextFactory<NexusCoreDbContext>
{
    public NexusCoreDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        NexusCoreEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<NexusCoreDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new NexusCoreDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../NexusCore.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
