using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using Shared.OpenTelemetry.Extensions;

namespace Shared.OpenTelemetry.Tests.Extensions;

[TestFixture]
public class WebApplicationBuilderExtensionsTests
{
    [Test]
    public void ConfigureLogging_ConfiguresOpenTelemetryLoggerOptions()
    {
        var builder = WebApplication.CreateBuilder();

        builder.ConfigureLogging("shared-open-telemetry-tests");

        using var provider = builder.Services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<OpenTelemetryLoggerOptions>>().Value;

        Assert.That(options.IncludeFormattedMessage, Is.True);
        Assert.That(options.IncludeScopes, Is.True);
        Assert.That(options.ParseStateValues, Is.True);
    }

    [Test]
    public void ConfigureLogging_ClearsExistingProvidersAndAddsConsoleAndOpenTelemetry()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.Services.AddSingleton<ILoggerProvider, TestLoggerProvider>();

        builder.ConfigureLogging("shared-open-telemetry-tests");

        using var provider = builder.Services.BuildServiceProvider();
        var loggerProviders = provider.GetServices<ILoggerProvider>().ToList();

        Assert.That(loggerProviders.Any(loggerProvider => loggerProvider is TestLoggerProvider), Is.False);
        Assert.That(loggerProviders.Any(loggerProvider => loggerProvider.GetType().Name.Contains("Console", StringComparison.Ordinal)), Is.True);
        Assert.That(loggerProviders.Any(loggerProvider => loggerProvider.GetType().Name.Contains("OpenTelemetry", StringComparison.Ordinal)), Is.True);
    }

    private sealed class TestLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return NullLogger.Instance;
        }

        public void Dispose()
        {
        }

        private sealed class NullLogger : ILogger
        {
            public static readonly NullLogger Instance = new();

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return false;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
            }
        }
    }
}