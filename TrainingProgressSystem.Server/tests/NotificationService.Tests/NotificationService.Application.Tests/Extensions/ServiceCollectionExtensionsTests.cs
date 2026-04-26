using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Application.Extensions;
using NotificationService.Application.Interfaces.v1;
using NotificationService.Application.Services.v1;

namespace NotificationService.Application.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void ConfigureApplicationServices_RegistersRemindersServiceAsScoped()
    {
        var services = new ServiceCollection();
        var logger = Mock.Of<ILogger>();

        services.ConfigureApplicationServices(logger);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IRemindersService));

        Assert.That(descriptor, Is.Not.Null);
        Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(RemindersService)));
        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }

    [Test]
    public void ConfigureApplicationServices_RegistersNotificationSyncServiceAsScoped()
    {
        var services = new ServiceCollection();
        var logger = Mock.Of<ILogger>();

        services.ConfigureApplicationServices(logger);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(INotificationSyncService));

        Assert.That(descriptor, Is.Not.Null);
        Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(NotificationSyncService)));
        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }
}
