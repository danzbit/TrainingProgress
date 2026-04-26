using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrainingService.Application.Interfaces.v1;
using TrainingService.Application.Services.v1;

namespace TrainingService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureApplicationServices(this IServiceCollection service, ILogger logger)
    {
        logger.LogInformation("Configuring Application Services");
        
        service.AddAutoMapper(cfg => cfg.AddMaps(typeof(ServiceCollectionExtensions).Assembly));
        logger.LogInformation("AutoMapper configured successfully");

        service.ConfigureServices();
        logger.LogInformation("Application services configured successfully");
    }

    public static void ConfigureServices(this IServiceCollection service)
    {
        service.AddScoped<IWorkoutService, WorkoutService>();
        service.AddScoped<IGoalService, GoalService>();
        service.AddScoped<IWorkoutTypeService, WorkoutTypeService>();
        service.AddScoped<IExerciseTypeService, ExerciseTypeService>();
        service.AddScoped<IUserPreferenceService, UserPreferenceService>();
        service.AddScoped<IAchievementService, AchievementService>();
    }
}