using AiChatService.Api.Extensions;
using AiChatService.Application.Extensions;
using AiChatService.Infrastructure.Data;
using AiChatService.Infrastructure.Extensions;
using Shared.Api.Extensions;
using Shared.OpenTelemetry.Extensions;

var loggerFactory = LoggerFactory.Create(config => config.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var applicationName = builder.Environment.ApplicationName;

    builder.ConfigureLogging(applicationName);

    builder.Services.ConfigureInfrastructureServices(builder.Configuration, logger);
    builder.Services.ConfigureApplicationServices(logger);
    builder.Services.ConfigureApiServices(builder.Configuration, applicationName, logger);

    var app = builder.Build();

    await app.ApplyMigrationsAsync<AiChatServiceDbContext>(logger);

    app.Configure(logger);

    await app.RunAsync();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "An unhandled exception occurred during bootstrapping");
}
