using AiChatService.Infrastructure.Data;
using Shared.Api.Extensions;
using Shared.Api.Middlewares;
using Shared.Auth.Extensions;
using Shared.OpenTelemetry.Extensions;

namespace AiChatService.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        ILogger logger)
    {
        logger.LogInformation("Configuring api services");

        services.ConfigureCurrentUser();
        logger.LogInformation("Configured current user");

        services.AddControllers();
        logger.LogInformation("Configured controllers");

        services.AddExceptionHandler<GlobalExceptionHandler>();
        logger.LogInformation("Configured exception handlers");

        services.ConfigureApiVersioning();
        logger.LogInformation("Configured api versioning");

        services.ConfigureProblemDetails();
        logger.LogInformation("Configured problem details");

        services.ConfigureSwagger();
        logger.LogInformation("Configured Swagger");

        services.ConfigureOpenTelemetry(serviceName);
        logger.LogInformation("Configured OpenTelemetry");

        services.AddHealthChecks();
        logger.LogInformation("Configured health checks");

        services.ConfigureCaching(configuration, serviceName);
        logger.LogInformation("Configured caching");

        services.ConfigureIdempotency();
        logger.LogInformation("Configured Idempotency");

        services.ConfigureDbContext<AiChatServiceDbContext>(configuration, "DefaultConnection");
        logger.LogInformation("Configured DbContext");

        services.ConfigureIdentity<AiChatServiceDbContext>();
        logger.LogInformation("Configured Identity");

        services.ConfigureJwtAuthentication(configuration);
        logger.LogInformation("Configured JWT authentication");

        services.AddCors(options =>
        {
            var bffUrl = Environment.GetEnvironmentVariable("BFF_URL") ?? "http://localhost:7000";
            var configuredOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
            var origins = (configuredOrigins?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [])
                .Append(bffUrl)
                .Append("http://localhost")
                .Append("http://localhost:80")
                .Append("http://127.0.0.1")
                .Append("http://127.0.0.1:80")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            options.AddPolicy("bff", p =>
                p.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });
        logger.LogInformation("Configured CORS for BFF");
    }
}
