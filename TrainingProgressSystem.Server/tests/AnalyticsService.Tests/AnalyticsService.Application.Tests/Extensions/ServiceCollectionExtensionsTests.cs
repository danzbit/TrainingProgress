using AnalyticsService.Application.Extensions;
using AnalyticsService.Application.Interfaces.v1;
using AnalyticsService.Application.Services.v1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace AnalyticsService.Application.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void ConfigureApplicationServices_RegistersExpectedServicesAsScoped()
    {
        var services = new ServiceCollection();
        var logger = Mock.Of<ILogger>();

        services.ConfigureApplicationServices(logger);

        var workoutDescriptor = services.FirstOrDefault(descriptor =>
            descriptor.ServiceType == typeof(IWorkoutAnalyticsService));
        var profileDescriptor = services.FirstOrDefault(descriptor =>
            descriptor.ServiceType == typeof(IProfileAnalyticsService));
        var snapshotDescriptor = services.FirstOrDefault(descriptor =>
            descriptor.ServiceType == typeof(IAnalyticsSnapshotService));
        var syncDescriptor = services.FirstOrDefault(descriptor =>
            descriptor.ServiceType == typeof(IAnalyticsSyncService));

        Assert.That(workoutDescriptor, Is.Not.Null);
        Assert.That(workoutDescriptor!.ImplementationType, Is.EqualTo(typeof(WorkoutAnalyticsService)));
        Assert.That(workoutDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));

        Assert.That(profileDescriptor, Is.Not.Null);
        Assert.That(profileDescriptor!.ImplementationType, Is.EqualTo(typeof(ProfileAnalyticsService)));
        Assert.That(profileDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));

        Assert.That(snapshotDescriptor, Is.Not.Null);
        Assert.That(snapshotDescriptor!.ImplementationType, Is.EqualTo(typeof(AnalyticsSnapshotService)));
        Assert.That(snapshotDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));

        Assert.That(syncDescriptor, Is.Not.Null);
        Assert.That(syncDescriptor!.ImplementationType, Is.EqualTo(typeof(AnalyticsSyncService)));
        Assert.That(syncDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }
}
