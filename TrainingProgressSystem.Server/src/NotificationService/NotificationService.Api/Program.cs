using NotificationService.Api.Extensions;
using NotificationService.Application.Extensions;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Extensions;
using Shared.Api.Extensions;
using Shared.OpenTelemetry.Extensions;

var loggerFactory = LoggerFactory.Create(config => config.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var applicationName = builder.Environment.ApplicationName;

    builder.ConfigureLogging(applicationName);
    
    builder.Services.ConfigureInfrastructureServices(logger);
    builder.Services.ConfigureApplicationServices(logger);
    builder.Services.ConfigureApiServices(builder, builder.Configuration, applicationName, logger);

    var app = builder.Build();

    await app.ApplyMigrationsAsync<NotificationServiceDbContext>(logger);

    app.Configure(logger);
    
    await app.RunAsync();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "An unhandled exception occurred during bootstrapping");
}

public partial class Program;