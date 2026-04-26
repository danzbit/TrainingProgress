using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Application.Extensions;

namespace Shared.Application.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void ConfigureAutoMapper_RegistersMapperServices()
    {
        var services = new ServiceCollection();

        services.ConfigureAutoMapper();

        using var provider = services.BuildServiceProvider();

        Assert.That(provider.GetService<AutoMapper.IMapper>(), Is.Not.Null);
        Assert.That(provider.GetService<AutoMapper.IConfigurationProvider>(), Is.Not.Null);
    }

    [Test]
    public void ConfigureOptionsWithEnvironmentFallback_BindsFromSection()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Sample:PrimaryValue"] = "configured-value"
        });

        services.ConfigureOptionsWithEnvironmentFallback<SampleOptions>(
            configuration,
            "Sample",
            new Dictionary<string, string>
            {
                [nameof(SampleOptions.PrimaryValue)] = "SAMPLE_PRIMARY"
            });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<SampleOptions>>().Value;

        Assert.That(options.PrimaryValue, Is.EqualTo("configured-value"));
    }

    [Test]
    public void ConfigureOptionsWithEnvironmentFallback_UsesEnvironmentWhenSectionValueMissing()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["SAMPLE_PRIMARY"] = "fallback-value"
        });

        services.ConfigureOptionsWithEnvironmentFallback<SampleOptions>(
            configuration,
            "Sample",
            new Dictionary<string, string>
            {
                [nameof(SampleOptions.PrimaryValue)] = "SAMPLE_PRIMARY"
            });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<SampleOptions>>().Value;

        Assert.That(options.PrimaryValue, Is.EqualTo("fallback-value"));
    }

    [Test]
    public void ConfigureOptionsWithEnvironmentFallback_DoesNotOverrideConfiguredValue()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Sample:PrimaryValue"] = "configured-value",
            ["SAMPLE_PRIMARY"] = "fallback-value"
        });

        services.ConfigureOptionsWithEnvironmentFallback<SampleOptions>(
            configuration,
            "Sample",
            new Dictionary<string, string>
            {
                [nameof(SampleOptions.PrimaryValue)] = "SAMPLE_PRIMARY"
            });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<SampleOptions>>().Value;

        Assert.That(options.PrimaryValue, Is.EqualTo("configured-value"));
    }

    [Test]
    public void ConfigureOptionsWithEnvironmentFallback_IgnoresInvalidMappings()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["SAMPLE_PRIMARY"] = "fallback-value"
        });

        services.ConfigureOptionsWithEnvironmentFallback<SampleOptions>(
            configuration,
            "Sample",
            new Dictionary<string, string>
            {
                [nameof(SampleOptions.NonStringValue)] = "SAMPLE_PRIMARY",
                ["MissingProperty"] = "SAMPLE_PRIMARY"
            });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<SampleOptions>>().Value;

        Assert.That(options.PrimaryValue, Is.Null);
    }

    private static IConfiguration BuildConfiguration(IReadOnlyDictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private sealed class SampleOptions
    {
        public string? PrimaryValue { get; set; }

        public int NonStringValue { get; set; }
    }
}