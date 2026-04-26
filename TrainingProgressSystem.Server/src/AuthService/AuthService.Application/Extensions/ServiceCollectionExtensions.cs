using AuthService.Application.Interfaces.v1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AuthService.Application.Services.v1;

namespace AuthService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureApplicationServices(this IServiceCollection service, ILogger logger)
    {
        logger.LogInformation("Configuring application services");

        service.AddScoped<IAuthService, Services.v1.AuthService>();
        logger.LogInformation("Application services configured successfully");

        logger.LogInformation("Configured application services");
    }
}