using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TrainingService.Api.Controllers.v1;
using TrainingService.Api.Grpc.v1;
using TrainingService.Application.Interfaces.v1;
using Shared.Grpc.Contracts;

namespace TrainingService.Api.Integration.Infrastructure;

public sealed class TrainingApiFactory : IAsyncDisposable
{
    private readonly IHost _host;
    private readonly TestServer _server;

    public StubWorkoutService WorkoutService { get; } = new();
    public StubGoalService GoalService { get; } = new();
    public StubAchievementService AchievementService { get; } = new();
    public StubWorkoutTypeService WorkoutTypeService { get; } = new();
    public StubExerciseTypeService ExerciseTypeService { get; } = new();
    public StubUserPreferenceService UserPreferenceService { get; } = new();

    public TrainingApiFactory()
    {
        _host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseEnvironment("Testing");
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddControllers()
                        .AddApplicationPart(typeof(WorkoutsController).Assembly)
                        .AddOData(options => options.Select().Filter().OrderBy().Count().SetMaxTop(100));

                    services.AddGrpc();

                    services.AddApiVersioning(options =>
                    {
                        options.AssumeDefaultVersionWhenUnspecified = true;
                        options.DefaultApiVersion = new ApiVersion(1, 0);
                        options.ReportApiVersions = true;
                        options.ApiVersionReader = ApiVersionReader.Combine(
                            new QueryStringApiVersionReader("api-version"),
                            new UrlSegmentApiVersionReader());
                    });
                    services.AddVersionedApiExplorer(options =>
                    {
                        options.GroupNameFormat = "'v'VVV";
                        options.SubstituteApiVersionInUrl = true;
                    });

                    services.AddSingleton(WorkoutService);
                    services.AddScoped<IWorkoutService>(sp => sp.GetRequiredService<StubWorkoutService>());

                    services.AddSingleton(GoalService);
                    services.AddScoped<IGoalService>(sp => sp.GetRequiredService<StubGoalService>());

                    services.AddSingleton(AchievementService);
                    services.AddScoped<IAchievementService>(sp => sp.GetRequiredService<StubAchievementService>());

                    services.AddSingleton(WorkoutTypeService);
                    services.AddScoped<IWorkoutTypeService>(sp => sp.GetRequiredService<StubWorkoutTypeService>());

                    services.AddSingleton(ExerciseTypeService);
                    services.AddScoped<IExerciseTypeService>(sp => sp.GetRequiredService<StubExerciseTypeService>());

                    services.AddSingleton(UserPreferenceService);
                    services.AddScoped<IUserPreferenceService>(sp => sp.GetRequiredService<StubUserPreferenceService>());

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
                        endpoints.MapGrpcService<TrainingSyncGrpcService>();
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

    public TrainingSyncGrpc.TrainingSyncGrpcClient CreateGrpcClient(out GrpcChannel channel)
    {
        var handler = _server.CreateHandler();
        channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpHandler = handler });
        return new TrainingSyncGrpc.TrainingSyncGrpcClient(channel);
    }

    public async ValueTask DisposeAsync()
    {
        _server.Dispose();
        await _host.StopAsync();
        _host.Dispose();
    }
}
