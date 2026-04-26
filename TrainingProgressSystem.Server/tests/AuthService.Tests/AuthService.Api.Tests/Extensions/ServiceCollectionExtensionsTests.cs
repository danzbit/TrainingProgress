using AuthService.Api.Extensions;
using AuthService.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthService.Api.Tests.Extensions;

[TestFixture]
[NonParallelizable]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void ConfigureApiServices_RegistersExpectedCoreServices()
    {
        var previousBffUrl = Environment.GetEnvironmentVariable("BFF_URL");
        Environment.SetEnvironmentVariable("BFF_URL", "http://localhost:7999");

        try
        {
            var builder = CreateBuilder();
            var logger = LoggerFactory.Create(logging => logging.SetMinimumLevel(LogLevel.Warning))
                .CreateLogger("tests");

            builder.Services.ConfigureApiServices(builder.Configuration, "AuthService.Api", logger);

            var provider = builder.Services.BuildServiceProvider();

            Assert.That(provider.GetService<HealthCheckService>(), Is.Not.Null);
            Assert.That(provider.GetService<IAuthenticationSchemeProvider>(), Is.Not.Null);
            Assert.That(provider.GetService<IDistributedCache>(), Is.Not.Null);
            Assert.That(provider.GetService<AuthServiceDbContext>(), Is.Not.Null);
            Assert.That(provider.GetService<UserManager<Shared.Infrastructure.Identity.ApplicationUser>>(), Is.Not.Null);

            var mvcOptions = provider.GetRequiredService<IOptions<MvcOptions>>();
            Assert.That(mvcOptions.Value, Is.Not.Null);

            var corsOptions = provider.GetRequiredService<IOptions<CorsOptions>>();
            var bffPolicy = corsOptions.Value.GetPolicy("bff");
            Assert.That(bffPolicy, Is.Not.Null);
            Assert.That(bffPolicy!.Origins, Contains.Item("http://localhost:7999"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("BFF_URL", previousBffUrl);
        }
    }

    [Test]
    public void ConfigureApiServices_WhenBffUrlNotSet_UsesDefaultOrigin()
    {
        var previousBffUrl = Environment.GetEnvironmentVariable("BFF_URL");
        Environment.SetEnvironmentVariable("BFF_URL", null);

        try
        {
            var builder = CreateBuilder();
            var logger = LoggerFactory.Create(logging => logging.SetMinimumLevel(LogLevel.Warning))
                .CreateLogger("tests");

            builder.Services.ConfigureApiServices(builder.Configuration, "AuthService.Api", logger);

            var provider = builder.Services.BuildServiceProvider();
            var corsOptions = provider.GetRequiredService<IOptions<CorsOptions>>();
            var bffPolicy = corsOptions.Value.GetPolicy("bff");

            Assert.That(bffPolicy, Is.Not.Null);
            Assert.That(bffPolicy!.Origins, Contains.Item("http://localhost:7000"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("BFF_URL", previousBffUrl);
        }
    }

    private static WebApplicationBuilder CreateBuilder()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development,
            ApplicationName = "AuthService.Api.Tests"
        });

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JwtSettings:Issuer"] = "TestsIssuer",
            ["JwtSettings:Audience"] = "TestsAudience",
            ["JwtSettings:SecretKey"] = "tests-secret-key-min-32-characters",
            ["ConnectionStrings:DefaultConnection"] =
                "Server=(localdb)\\MSSQLLocalDB;Database=AuthApiTests;Trusted_Connection=True;"
        });

        return builder;
    }
}