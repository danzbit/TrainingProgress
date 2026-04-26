using Shared.OpenTelemetry.Extensions;
using Shared.Api.Extensions;
using TrainingService.Api.Extensions;
using TrainingService.Application.Extensions;
using TrainingService.Infrastructure.Data;
using TrainingService.Infrastructure.Extensions;

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

    await app.ApplyMigrationsAsync<TrainingServiceDbContext>(logger);

    app.Configure(logger);
    
    await app.RunAsync();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "An unhandled exception occurred during bootstrapping");
}