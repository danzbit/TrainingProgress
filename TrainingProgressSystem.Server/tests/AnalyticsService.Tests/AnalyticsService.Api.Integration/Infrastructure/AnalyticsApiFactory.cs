using AnalyticsService.Api.Controllers.v1;
using AnalyticsService.Api.Grpc.v1;
using AnalyticsService.Application.Interfaces.v1;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Grpc.Contracts;

namespace AnalyticsService.Api.Integration.Infrastructure;

public sealed class AnalyticsApiFactory : IDisposable
{
    private readonly IHost _host;
    private readonly TestServer _server;

    public StubWorkoutAnalyticsService WorkoutAnalyticsService { get; } = new();
    public StubProfileAnalyticsService ProfileAnalyticsService { get; } = new();
    public StubAnalyticsSyncService AnalyticsSyncService { get; } = new();

    public AnalyticsApiFactory()
    {
        _host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseEnvironment("Testing");
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddControllers().AddApplicationPart(typeof(WorkoutAnalyticsController).Assembly);
                    services.AddGrpc();
                    services.AddApiVersioning();

                    services.AddSingleton(WorkoutAnalyticsService);
                    services.AddScoped<IWorkoutAnalyticsService>(sp =>
                        sp.GetRequiredService<StubWorkoutAnalyticsService>());

                    services.AddSingleton(ProfileAnalyticsService);
                    services.AddScoped<IProfileAnalyticsService>(sp =>
                        sp.GetRequiredService<StubProfileAnalyticsService>());

                    services.AddSingleton(AnalyticsSyncService);
                    services.AddScoped<IAnalyticsSyncService>(sp =>
                        sp.GetRequiredService<StubAnalyticsSyncService>());

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
                        endpoints.MapGrpcService<AnalyticsSyncGrpcService>();
                    });
                });
            })
            .Start();

        _server = _host.GetTestServer();
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = _server.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, "true");
        return client;
    }

    public HttpClient CreateClient()
    {
        return _server.CreateClient();
    }

    public AnalyticsSyncGrpc.AnalyticsSyncGrpcClient CreateGrpcClient(out GrpcChannel channel)
    {
        var handler = _server.CreateHandler();
        channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpHandler = handler });
        return new AnalyticsSyncGrpc.AnalyticsSyncGrpcClient(channel);
    }

    public void Dispose()
    {
        _server.Dispose();
        _host.StopAsync().GetAwaiter().GetResult();
        _host.Dispose();
    }
}
