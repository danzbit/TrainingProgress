using AnalyticsService.Domain.Interfaces.v1;
using AnalyticsService.Infrastructure.Extensions;
using AnalyticsService.Infrastructure.Repositories.v1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace AnalyticsService.Infrastructure.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void ConfigureInfrastructureServices_RegistersWorkoutRepositoryAsScoped()
    {
        var services = new ServiceCollection();
        var logger = Mock.Of<ILogger>();

        services.ConfigureInfrastructureServices(logger);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IWorkoutRepository));
        var snapshotDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IAnalyticsSnapshotRepository));

        Assert.That(descriptor, Is.Not.Null);
        Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(WorkoutRepository)));
        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));

        Assert.That(snapshotDescriptor, Is.Not.Null);
        Assert.That(snapshotDescriptor!.ImplementationType, Is.EqualTo(typeof(AnalyticsSnapshotRepository)));
        Assert.That(snapshotDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }
}
