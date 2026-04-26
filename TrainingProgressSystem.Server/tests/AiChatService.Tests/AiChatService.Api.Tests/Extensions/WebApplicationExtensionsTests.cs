using AiChatService.Api.Extensions;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiChatService.Api.Tests.Extensions;

[TestFixture]
[NonParallelizable]
public class WebApplicationExtensionsTests
{
    [Test]
    public async Task Configure_MapsHealthEndpoint()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development,
            ApplicationName = "AiChatService.Api.Tests"
        });

        builder.WebHost.UseTestServer();

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JwtSettings:Issuer"] = "TestsIssuer",
            ["JwtSettings:Audience"] = "TestsAudience",
            ["JwtSettings:SecretKey"] = "tests-secret-key-min-32-characters",
            ["ConnectionStrings:DefaultConnection"] =
                "Server=(localdb)\\MSSQLLocalDB;Database=AiChatApiTests;Trusted_Connection=True;"
        });

        var logger = LoggerFactory.Create(logging => logging.SetMinimumLevel(LogLevel.Warning))
            .CreateLogger("tests");

        builder.Services.ConfigureApiServices(builder.Configuration, "AiChatService.Api", logger);

        var app = builder.Build();
        app.Configure(logger);
        await app.StartAsync();

        try
        {
            var client = app.GetTestClient();
            var healthResponse = await client.GetAsync("/health");

            Assert.That(healthResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var endpoints = app.Services.GetServices<EndpointDataSource>()
                .SelectMany(source => source.Endpoints)
                .ToList();

            Assert.That(endpoints.Any(e => e.DisplayName?.Contains("health", StringComparison.OrdinalIgnoreCase) == true),
                Is.True,
                "Expected /health endpoint to be mapped.");
        }
        finally
        {
            await app.StopAsync();
            await app.DisposeAsync();
        }
    }
}
