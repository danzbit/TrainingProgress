using Bff.Application.Extensions;
using Bff.Application.Interfaces.v1;
using Bff.Application.Services.v1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bff.Application.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    private static ServiceDescriptor? FindDescriptor<TService>(IServiceCollection services)
    {
        return services.SingleOrDefault(d => d.ServiceType == typeof(TService));
    }

    [Test]
    public void ConfigureApplicationServices_RegistersCreateWorkoutSagaOrchestrator()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerMock = new Mock<ILogger>();

        // Act
        services.ConfigureApplicationServices(loggerMock.Object);
        // Assert
        var descriptor = FindDescriptor<ICreateWorkoutSagaOrchestrator>(services);
        Assert.That(descriptor, Is.Not.Null);
        Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(CreateWorkoutSagaOrchestrator)));
        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }

    [Test]
    public void ConfigureApplicationServices_RegistersSaveGoalSagaOrchestrator()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerMock = new Mock<ILogger>();

        // Act
        services.ConfigureApplicationServices(loggerMock.Object);
        // Assert
        var descriptor = FindDescriptor<ISaveGoalSagaOrchestrator>(services);
        Assert.That(descriptor, Is.Not.Null);
        Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(SaveGoalSagaOrchestrator)));
        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }

    [Test]
    public void ConfigureApplicationServices_RegistersAsScoped_ForCreateWorkoutOrchestrator()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerMock = new Mock<ILogger>();

        // Act
        services.ConfigureApplicationServices(loggerMock.Object);
        // Assert
        var descriptor = FindDescriptor<ICreateWorkoutSagaOrchestrator>(services);
        Assert.That(descriptor, Is.Not.Null);
        Assert.That(descriptor!.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }

    [Test]
    public void ConfigureApplicationServices_RegistersAsScoped_ForSaveGoalOrchestrator()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerMock = new Mock<ILogger>();

        // Act
        services.ConfigureApplicationServices(loggerMock.Object);
        // Assert
        var descriptor = FindDescriptor<ISaveGoalSagaOrchestrator>(services);
        Assert.That(descriptor, Is.Not.Null);
        Assert.That(descriptor!.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }

    [Test]
    public void ConfigureApplicationServices_RegistersOnlyExpectedOrchestrators()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerMock = new Mock<ILogger>();

        // Act
        services.ConfigureApplicationServices(loggerMock.Object);
        // Assert
        var createWorkoutCount = services.Count(d => d.ServiceType == typeof(ICreateWorkoutSagaOrchestrator));
        var saveGoalCount = services.Count(d => d.ServiceType == typeof(ISaveGoalSagaOrchestrator));

        Assert.That(createWorkoutCount, Is.EqualTo(1));
        Assert.That(saveGoalCount, Is.EqualTo(1));
    }

    [Test]
    public void ConfigureApplicationServices_RegistersAllRequiredOrchestrators()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerMock = new Mock<ILogger>();

        // Act
        services.ConfigureApplicationServices(loggerMock.Object);
        // Assert
        var createWorkoutDescriptor = FindDescriptor<ICreateWorkoutSagaOrchestrator>(services);
        var saveGoalDescriptor = FindDescriptor<ISaveGoalSagaOrchestrator>(services);

        Assert.That(createWorkoutDescriptor, Is.Not.Null);
        Assert.That(saveGoalDescriptor, Is.Not.Null);
    }
}
