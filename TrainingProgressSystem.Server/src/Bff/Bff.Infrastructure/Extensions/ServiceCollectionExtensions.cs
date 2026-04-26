using Bff.Application.Interfaces.v1;
using Bff.Infrastructure.Clients;
using Bff.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Application.Extensions;
using Shared.Grpc.Contracts;

namespace Bff.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureInfrastructureServices(this IServiceCollection services, IConfiguration configuration,
        ILogger logger)
    {
        logger.LogInformation("Configuring Bff infrastructure services");

        services.ConfigureOptionsWithEnvironmentFallback<DownstreamServicesOptions>(
                configuration,
                DownstreamServicesOptions.SectionName,
                new Dictionary<string, string>
                {
                    [nameof(DownstreamServicesOptions.TrainingGrpcUrl)] = "TRAINING_SERVICE_GRPC_URL",
                    [nameof(DownstreamServicesOptions.AnalyticsGrpcUrl)] = "ANALYTICS_SERVICE_GRPC_URL",
                    [nameof(DownstreamServicesOptions.NotificationGrpcUrl)] = "NOTIFICATION_SERVICE_GRPC_URL"
                })
            .Validate(options => !string.IsNullOrWhiteSpace(options.TrainingGrpcUrl), "Training gRPC URL is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.AnalyticsGrpcUrl), "Analytics gRPC URL is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.NotificationGrpcUrl), "Notification gRPC URL is required")
            .ValidateOnStart();
        logger.LogInformation("Configured downstream services options");

        services.ConfigureGrpcClients();
        logger.LogInformation("Configured gRPC clients for downstream services");

        logger.LogInformation("Bff infrastructure services configured");
    }

    private static void ConfigureGrpcClients(this IServiceCollection services)
    {
        services.AddGrpcClient<TrainingSyncGrpc.TrainingSyncGrpcClient>((provider, options) =>
        {
            var settings = provider.GetRequiredService<IOptions<DownstreamServicesOptions>>().Value;
            options.Address = new Uri(settings.TrainingGrpcUrl);
        });
        services.AddScoped<ITrainingSyncClient, TrainingSyncGrpcClient>();

        services.AddGrpcClient<AnalyticsSyncGrpc.AnalyticsSyncGrpcClient>((provider, options) =>
        {
            var settings = provider.GetRequiredService<IOptions<DownstreamServicesOptions>>().Value;
            options.Address = new Uri(settings.AnalyticsGrpcUrl);
        });
        services.AddScoped<IAnalyticsSyncClient, AnalyticsSyncGrpcClient>();

        services.AddGrpcClient<NotificationSyncGrpc.NotificationSyncGrpcClient>((provider, options) =>
        {
            var settings = provider.GetRequiredService<IOptions<DownstreamServicesOptions>>().Value;
            options.Address = new Uri(settings.NotificationGrpcUrl);
        });
        services.AddScoped<INotificationSyncClient, NotificationSyncGrpcClient>();
    }
}
