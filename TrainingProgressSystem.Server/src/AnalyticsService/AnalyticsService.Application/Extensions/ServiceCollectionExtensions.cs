using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AnalyticsService.Application.Interfaces.v1;
using AnalyticsService.Application.Services.v1;

namespace AnalyticsService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureApplicationServices(this IServiceCollection service, ILogger logger)
    {
        logger.LogInformation("Configuring Application Services");

        service.ConfigureServices();

        logger.LogInformation("Application services configured successfully");
    }

    private static void ConfigureServices(this IServiceCollection service)
    {
        service.AddScoped<IWorkoutAnalyticsService, WorkoutAnalyticsService>();
        service.AddScoped<IProfileAnalyticsService, ProfileAnalyticsService>();
        service.AddScoped<IAnalyticsSnapshotService, AnalyticsSnapshotService>();
        service.AddScoped<IAnalyticsSyncService, AnalyticsSyncService>();
    }
}