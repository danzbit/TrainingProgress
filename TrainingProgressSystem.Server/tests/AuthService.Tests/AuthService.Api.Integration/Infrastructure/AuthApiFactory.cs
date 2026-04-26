using AuthService.Api.Controllers.v1;
using AuthService.Application.Interfaces.v1;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Abstractions.Idempotency;

namespace AuthService.Api.Integration.Infrastructure;

public sealed class AuthApiFactory : IDisposable
{
    private readonly IHost _host;
    private readonly TestServer _server;

    public StubAuthService AuthService { get; } = new();
    internal InMemoryIdempotencyService IdempotencyService { get; } = new();

    public AuthApiFactory()
    {
        _host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseEnvironment("Testing");
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddControllers().AddApplicationPart(typeof(AuthController).Assembly);
                    services.AddApiVersioning();

                    services.AddSingleton(AuthService);
                    services.AddScoped<IAuthService>(sp => sp.GetRequiredService<StubAuthService>());

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

                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });
            })
            .Start();

        _server = _host.GetTestServer();
    }

    public HttpClient CreateAuthenticatedClient(Guid? userId = null)
    {
        var client = _server.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, "true");

        if (userId.HasValue)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeaderName, userId.Value.ToString());
        }

        return client;
    }

    public HttpClient CreateAuthenticatedClient(string userId)
    {
        var client = _server.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeaderName, userId);
        return client;
    }

    public HttpClient CreateClient()
    {
        return _server.CreateClient();
    }

    public void Dispose()
    {
        _server.Dispose();
        _host.StopAsync().GetAwaiter().GetResult();
        _host.Dispose();
    }
}
