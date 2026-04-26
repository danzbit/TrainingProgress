using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureInfrastructureServices(this IServiceCollection services, ILogger logger)
    {
        logger.LogInformation("Configured infrastructure services");
    }
}