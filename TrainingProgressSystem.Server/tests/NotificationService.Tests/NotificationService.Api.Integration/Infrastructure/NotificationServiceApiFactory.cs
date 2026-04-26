using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.Api.Controllers.v1;
using NotificationService.Api.Grpc.v1;
using NotificationService.Application.Interfaces.v1;
using Shared.Grpc.Contracts;

namespace NotificationService.Api.Integration.Infrastructure;

public sealed class NotificationServiceApiFactory : IAsyncDisposable
{
    private readonly IHost _host;
    private readonly TestServer _server;

    public StubNotificationSyncService NotificationSyncService { get; } = new();

    public StubRemindersService RemindersService { get; } = new();

    public NotificationServiceApiFactory()
    {
        _host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseEnvironment("Testing");
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddControllers().AddApplicationPart(typeof(RemindersController).Assembly);
                    services.AddGrpc();
                    services.AddApiVersioning();

                    services.AddSingleton(NotificationSyncService);
                    services.AddScoped<INotificationSyncService>(sp =>
                        sp.GetRequiredService<StubNotificationSyncService>());

                    services.AddSingleton(RemindersService);
                    services.AddScoped<IRemindersService>(sp => sp.GetRequiredService<StubRemindersService>());

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
                        endpoints.MapGrpcService<NotificationSyncGrpcService>();
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
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeaderName, (userId ?? Guid.NewGuid()).ToString());
        return client;
    }

    public HttpClient CreateClient()
    {
        return _server.CreateClient();
    }

    public NotificationSyncGrpc.NotificationSyncGrpcClient CreateGrpcClient(out GrpcChannel channel)
    {
        var handler = _server.CreateHandler();
        channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpHandler = handler });
        return new NotificationSyncGrpc.NotificationSyncGrpcClient(channel);
    }

    public async ValueTask DisposeAsync()
    {
        _server.Dispose();
        await _host.StopAsync();
        _host.Dispose();
    }
}
