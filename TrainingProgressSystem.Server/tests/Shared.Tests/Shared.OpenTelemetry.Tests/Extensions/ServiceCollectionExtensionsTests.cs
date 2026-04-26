using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Shared.OpenTelemetry.Extensions;

namespace Shared.OpenTelemetry.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void ConfigureOpenTelemetry_RegistersTracerAndMeterProviders()
    {
        var services = new ServiceCollection();

        services.ConfigureOpenTelemetry("shared-open-telemetry-tests");

        using var provider = services.BuildServiceProvider();
        var tracerProvider = provider.GetService<TracerProvider>();
        var meterProvider = provider.GetService<MeterProvider>();

        Assert.That(tracerProvider, Is.Not.Null);
        Assert.That(meterProvider, Is.Not.Null);
    }
}