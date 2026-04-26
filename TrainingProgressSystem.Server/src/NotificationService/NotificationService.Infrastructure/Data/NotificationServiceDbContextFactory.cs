using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace NotificationService.Infrastructure.Data;

/// <summary>
/// Design-time factory for NotificationServiceDbContext.
/// Used by EF Core tools to create DbContext instances during migrations and scaffolding.
/// </summary>
public class NotificationServiceDbContextFactory : IDesignTimeDbContextFactory<NotificationServiceDbContext>
{
    private const string DefaultConnectionStringName = "DefaultConnection";
    private const string AppsettingsFileName = "appsettings.json";

    public NotificationServiceDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var connectionString = GetConnectionString(configuration);
        var optionsBuilder = new DbContextOptionsBuilder<NotificationServiceDbContext>();
        
        optionsBuilder.UseSqlServer(connectionString);

        return new NotificationServiceDbContext(optionsBuilder.Options);
    }

    private static IConfiguration BuildConfiguration()
    {
        var basePath = GetConfigurationPath();

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile(AppsettingsFileName, optional: false, reloadOnChange: false)
            .Build();
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DefaultConnectionStringName);
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{DefaultConnectionStringName}' not found in configuration.");
        }

        return connectionString;
    }

    private static string GetConfigurationPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        
        if (Directory.Exists(Path.Combine(currentDirectory, "src", "NotificationService", "NotificationService.Api")))
        {
            return Path.Combine(currentDirectory, "src", "NotificationService", "NotificationService.Api");
        }
        
        if (File.Exists(Path.Combine(currentDirectory, AppsettingsFileName)))
        {
            return currentDirectory;
        }

        throw new InvalidOperationException(
            $"Could not find appsettings.json. Ensure migrations are run from the solution root directory or the API project directory.");
    }
}