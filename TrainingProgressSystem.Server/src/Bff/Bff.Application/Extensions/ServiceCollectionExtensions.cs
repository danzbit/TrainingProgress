using Bff.Application.Interfaces.v1;
using Bff.Application.Services.v1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bff.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureApplicationServices(this IServiceCollection services, ILogger logger)
    {
        logger.LogInformation("Configuring Bff application services");

        services.AddScoped<ICreateWorkoutSagaOrchestrator, CreateWorkoutSagaOrchestrator>();
        services.AddScoped<ISaveGoalSagaOrchestrator, SaveGoalSagaOrchestrator>();

        logger.LogInformation("Bff application services configured");
    }
}
