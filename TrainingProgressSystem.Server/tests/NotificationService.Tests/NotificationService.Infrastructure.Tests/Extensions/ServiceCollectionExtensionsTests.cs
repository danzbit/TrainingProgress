using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Interfaces;
using NotificationService.Infrastructure.Extensions;
using NotificationService.Infrastructure.Repositories;

namespace NotificationService.Infrastructure.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void ConfigureInfrastructureServices_RegistersRemindersRepositoryAsScoped()
    {
        var services = new ServiceCollection();
        var logger = new TestLogger();

        services.ConfigureInfrastructureServices(logger);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IRemindersRepository));

        Assert.That(descriptor, Is.Not.Null);
        Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(RemindersRepository)));
        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }

    [Test]
    public void ConfigureInfrastructureServices_WritesExpectedLogMessages()
    {
        var services = new ServiceCollection();
        var logger = new TestLogger();

        services.ConfigureInfrastructureServices(logger);

        Assert.That(logger.Messages, Has.Some.Contains("Registered respositories"));
        Assert.That(logger.Messages, Has.Some.Contains("Configured infrastructure services"));
    }

    private sealed class TestLogger : ILogger
    {
        public List<string> Messages { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }
}
