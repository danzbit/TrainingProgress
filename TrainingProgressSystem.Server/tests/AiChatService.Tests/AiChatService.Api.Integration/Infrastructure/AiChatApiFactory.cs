using AiChatService.Application.Interfaces.v1;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Abstractions.Idempotency;

namespace AiChatService.Api.Integration.Infrastructure;

public sealed class AiChatApiFactory : WebApplicationFactory<Program>
{
    public StubChatService ChatService { get; } = new();
    internal InMemoryIdempotencyService IdempotencyService { get; } = new();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Issuer"] = "IntegrationTests",
                ["JwtSettings:Audience"] = "IntegrationTestsAudience",
                ["JwtSettings:SecretKey"] = "integration-tests-secret-key-min-32-chars",
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=(localdb)\\MSSQLLocalDB;Database=AiChatIntegrationTests;Trusted_Connection=True;",
                ["Groq:ApiKey"] = "integration-test-api-key",
                ["Groq:Model"] = "integration-test-model"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IChatService>();
            services.RemoveAll<IIdempotencyService>();

            services.AddSingleton(ChatService);
            services.AddScoped<IChatService>(sp => sp.GetRequiredService<StubChatService>());

            services.AddSingleton(IdempotencyService);
            services.AddScoped<IIdempotencyService>(sp => sp.GetRequiredService<InMemoryIdempotencyService>());

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            services.AddAuthorization();
        });
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, "true");
        return client;
    }
}
