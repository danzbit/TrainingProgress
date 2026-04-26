using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TrainingService.Domain.Interfaces.v1;
using TrainingService.Infrastructure.Extensions;
using TrainingService.Infrastructure.Repositories.v1;

namespace TrainingService.Infrastructure.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void ConfigureInfrastructureServices_RegistersRepositoriesAsScoped()
    {
        var services = new ServiceCollection();
        var logger = Mock.Of<ILogger>();

        services.ConfigureInfrastructureServices(logger);

        AssertScoped<IWorkoutRepository, WorkoutRepository>(services);
        AssertScoped<IGoalRepository, GoalRepository>(services);
        AssertScoped<IWorkoutTypeRepository, WorkoutTypeRepository>(services);
        AssertScoped<IExerciseTypeRepository, ExerciseTypeRepository>(services);
        AssertScoped<IUserPreferenceRepository, UserPreferenceRepository>(services);
        AssertScoped<IAchievementRepository, AchievementRepository>(services);
    }

    private static void AssertScoped<TService, TImplementation>(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(TService));

        Assert.That(descriptor, Is.Not.Null);
        Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(TImplementation)));
        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }
}