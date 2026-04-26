using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TrainingService.Application.Extensions;
using TrainingService.Application.Interfaces.v1;
using TrainingService.Application.Services.v1;

namespace TrainingService.Application.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void ConfigureServices_RegistersAllApplicationServicesAsScoped()
    {
        var services = new ServiceCollection();

        services.ConfigureServices();

        AssertScoped<IWorkoutService, WorkoutService>(services);
        AssertScoped<IGoalService, GoalService>(services);
        AssertScoped<IWorkoutTypeService, WorkoutTypeService>(services);
        AssertScoped<IExerciseTypeService, ExerciseTypeService>(services);
        AssertScoped<IUserPreferenceService, UserPreferenceService>(services);
        AssertScoped<IAchievementService, AchievementService>(services);
    }

    [Test]
    public void ConfigureApplicationServices_RegistersAutoMapper()
    {
        var services = new ServiceCollection();
        var logger = new Mock<ILogger>();

        services.ConfigureApplicationServices(logger.Object);

        var mapperDescriptor = services.FirstOrDefault(d => d.ServiceType.FullName == "AutoMapper.IMapper");
        Assert.That(mapperDescriptor, Is.Not.Null);
    }

    private static void AssertScoped<TService, TImplementation>(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(TService));

        Assert.That(descriptor, Is.Not.Null);
        Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(TImplementation)));
        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }
}
