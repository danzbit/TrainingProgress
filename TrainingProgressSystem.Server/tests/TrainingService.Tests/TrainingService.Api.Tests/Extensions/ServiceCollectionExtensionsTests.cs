using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using TrainingService.Api.Extensions;

namespace TrainingService.Api.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public async Task ConfigureApiServices_RegistersCoreApiServices()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=TrainingServiceApiTests;Trusted_Connection=True;TrustServerCertificate=True";
        builder.Configuration["JwtSettings:SecretKey"] = "SuperSecretKey1234567890SuperSecretKey1234567890";
        builder.Configuration["JwtSettings:Issuer"] = "training-tests";
        builder.Configuration["JwtSettings:Audience"] = "training-tests";

        var logger = new Mock<ILogger>().Object;

        builder.Services.ConfigureApiServices(builder, builder.Configuration, "TrainingService.Api", logger);

        await using var provider = builder.Services.BuildServiceProvider();

        Assert.That(provider.GetService<IExceptionHandler>(), Is.Not.Null);
        Assert.That(provider.GetService<HealthCheckService>(), Is.Not.Null);
        Assert.That(provider.GetService<IAuthenticationSchemeProvider>(), Is.Not.Null);
        Assert.That(provider.GetService<ICorsService>(), Is.Not.Null);

        var corsOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CorsOptions>>().Value;
        var hasBffPolicy = corsOptions.GetPolicy("bff") is not null;
        Assert.That(hasBffPolicy, Is.True);
    }
}
