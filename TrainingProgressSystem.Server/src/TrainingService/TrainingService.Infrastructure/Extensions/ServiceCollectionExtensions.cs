using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrainingService.Domain.Interfaces.v1;
using TrainingService.Infrastructure.Repositories.v1;

namespace TrainingService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureInfrastructureServices(this IServiceCollection services, ILogger logger)
    {
        logger.LogInformation("Configured infrastructure services");

        ConfigureRepositories(services);
        logger.LogInformation("Configured repositories");
    }

    private static void ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IWorkoutRepository, WorkoutRepository>();
        services.AddScoped<IGoalRepository, GoalRepository>();
        services.AddScoped<IWorkoutTypeRepository, WorkoutTypeRepository>();
        services.AddScoped<IExerciseTypeRepository, ExerciseTypeRepository>();
        services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
        services.AddScoped<IAchievementRepository, AchievementRepository>();
    }
}