using AnalyticsService.Api.Extensions;
using AnalyticsService.Api.Grpc.v1;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AnalyticsService.Api.Tests.Extensions;

[TestFixture]
[NonParallelizable]
public class WebApplicationExtensionsTests
{
    [Test]
    public async Task Configure_MapsHealthEndpointAndGrpcEndpoints()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development,
            ApplicationName = "AnalyticsService.Api.Tests"
        });

        builder.WebHost.UseTestServer();

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JwtSettings:Issuer"] = "TestsIssuer",
            ["JwtSettings:Audience"] = "TestsAudience",
            ["JwtSettings:SecretKey"] = "tests-secret-key-min-32-characters",
            ["ConnectionStrings:DefaultConnection"] =
                "Server=(localdb)\\MSSQLLocalDB;Database=AnalyticsApiTests;Trusted_Connection=True;"
        });

        var logger = LoggerFactory.Create(logging => logging.SetMinimumLevel(LogLevel.Warning))
            .CreateLogger("tests");

        builder.Services.ConfigureApiServices(builder, builder.Configuration, "AnalyticsService.Api", logger);

        var app = builder.Build();
        app.Configure(logger);
        await app.StartAsync();

        try
        {
            var client = app.GetTestClient();
            var healthResponse = await client.GetAsync("/health");

            Assert.That(healthResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var endpointSources = app.Services.GetServices<EndpointDataSource>();
            var endpoints = endpointSources.SelectMany(source => source.Endpoints).ToList();

            Assert.That(endpoints.Any(endpoint => endpoint.DisplayName?.Contains("gRPC", StringComparison.OrdinalIgnoreCase) == true),
                Is.True,
                "Expected at least one mapped gRPC endpoint.");

            Assert.That(typeof(AnalyticsSyncGrpcService), Is.Not.Null);
        }
        finally
        {
            await app.StopAsync();
            await app.DisposeAsync();
        }
    }
}
