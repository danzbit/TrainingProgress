using Bff.Api.Extensions;
using Bff.Application.Extensions;
using Bff.Infrastructure.Extensions;
using Shared.OpenTelemetry.Extensions;

var loggerFactory = LoggerFactory.Create(config => config.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

try
{
	var builder = WebApplication.CreateBuilder(args);
	var applicationName = builder.Environment.ApplicationName;

	builder.ConfigureLogging(applicationName);

	builder.Services.ConfigureApplicationServices(logger);
	builder.Services.ConfigureInfrastructureServices(builder.Configuration, logger);
	builder.Services.ConfigureBffApiServices(applicationName, builder.Configuration);

	var app = builder.Build();

	app.Configure();

	await app.RunAsync();
}
catch (Exception ex)
{
	logger.LogCritical(ex, "An unhandled exception occurred during bootstrapping");
}