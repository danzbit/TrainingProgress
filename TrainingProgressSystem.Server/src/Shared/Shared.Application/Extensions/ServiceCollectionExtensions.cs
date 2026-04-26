using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Shared.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureAutoMapper(this IServiceCollection service)
    {
        service.AddAutoMapper(cfg => cfg.AddMaps(typeof(ServiceCollectionExtensions).Assembly));
    }

    public static OptionsBuilder<TOptions> ConfigureOptionsWithEnvironmentFallback<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName,
        IReadOnlyDictionary<string, string> propertyEnvironmentMappings)
        where TOptions : class, new()
    {
        return services
            .AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName))
            .PostConfigure(options =>
                ApplyEnvironmentFallback(options, configuration, propertyEnvironmentMappings));
    }

    private static void ApplyEnvironmentFallback<TOptions>(
        TOptions options,
        IConfiguration configuration,
        IReadOnlyDictionary<string, string> propertyEnvironmentMappings)
        where TOptions : class
    {
        var optionType = typeof(TOptions);

        foreach (var mapping in propertyEnvironmentMappings)
        {
            var property = optionType.GetProperty(mapping.Key);
            if (property is null || property.PropertyType != typeof(string) || !property.CanRead || !property.CanWrite)
            {
                continue;
            }

            var currentValue = property.GetValue(options) as string;
            if (!string.IsNullOrWhiteSpace(currentValue))
            {
                continue;
            }

            var environmentValue = configuration[mapping.Value];
            if (string.IsNullOrWhiteSpace(environmentValue))
            {
                continue;
            }

            property.SetValue(options, environmentValue);
        }
    }
}