using AuthService.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void ConfigureInfrastructureServices_LogsConfiguredMessage()
    {
        var services = new ServiceCollection();
        var logger = new ListLogger();

        services.ConfigureInfrastructureServices(logger);

        Assert.That(logger.Messages, Has.Count.EqualTo(1));
        Assert.That(logger.Messages[0], Is.EqualTo("Configured infrastructure services"));
    }

    private sealed class ListLogger : ILogger
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Information)
            {
                Messages.Add(formatter(state, exception));
            }
        }
    }
}