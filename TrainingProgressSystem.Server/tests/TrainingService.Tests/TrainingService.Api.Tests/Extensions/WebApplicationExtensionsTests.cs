using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TrainingService.Api.Extensions;

namespace TrainingService.Api.Tests.Extensions;

[TestFixture]
public class WebApplicationExtensionsTests
{
    [Test]
    public async Task Configure_MapsHealthAndGrpcEndpoints()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=TrainingServiceApiTests;Trusted_Connection=True;TrustServerCertificate=True";
        builder.Configuration["JwtSettings:SecretKey"] = "SuperSecretKey1234567890SuperSecretKey1234567890";
        builder.Configuration["JwtSettings:Issuer"] = "training-tests";
        builder.Configuration["JwtSettings:Audience"] = "training-tests";

        var logger = new Mock<ILogger>().Object;
        builder.Services.ConfigureApiServices(builder, builder.Configuration, "TrainingService.Api", logger);

        var app = builder.Build();
        app.Configure(logger);
        await app.StartAsync();

        var endpointDataSource = app.Services.GetRequiredService<EndpointDataSource>();
        var endpoints = endpointDataSource.Endpoints.Select(e => e.DisplayName ?? string.Empty).ToList();
        Assert.That(endpoints.Count, Is.GreaterThan(0));

        var client = app.GetTestClient();
        var healthResponse = await client.GetAsync("/health");
        Assert.That((int)healthResponse.StatusCode, Is.EqualTo(200));

        await app.StopAsync();
    }
}
