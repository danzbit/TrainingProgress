using Shared.Api.Extensions;
using AnalyticsService.Api.Grpc.v1;

namespace AnalyticsService.Api.Extensions;

public static class WebApplicationExtensions
{
    public static void Configure(this WebApplication app, ILogger logger)
    {
        logger.LogInformation("Configuring web application");

        app.UseCors("bff");
        logger.LogInformation("Configured CORS for BFF");

        app.ConfigureSwagger();
        logger.LogInformation("Configured Swagger");

        app.ConfigureApiMiddlewares();
        logger.LogInformation("Configured API middlewares");

        app.ConfigureWebApplication();
        logger.LogInformation("Configured web application");

        app.MapHealthChecks("/health");
        logger.LogInformation("Configured health checks");

        app.MapGrpcService<AnalyticsSyncGrpcService>();
        logger.LogInformation("Configured gRPC endpoints");
    }
}