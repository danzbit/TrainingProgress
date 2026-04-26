using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AnalyticsService.Domain.Interfaces.v1;
using AnalyticsService.Infrastructure.Repositories.v1;

namespace AnalyticsService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureInfrastructureServices(this IServiceCollection service, ILogger logger)
    {
        logger.LogInformation("Configured infrastructure services");

        service.ConfigureRepositories();

        logger.LogInformation("Configured repositories");
    }

    private static void ConfigureRepositories(this IServiceCollection service)
    {
        service.AddScoped<IWorkoutRepository, WorkoutRepository>();
        service.AddScoped<IAnalyticsSnapshotRepository, AnalyticsSnapshotRepository>();
    }
}