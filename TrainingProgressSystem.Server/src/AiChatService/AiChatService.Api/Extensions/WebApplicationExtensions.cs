using Shared.Api.Extensions;

namespace AiChatService.Api.Extensions;

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
    }
}
