using Bff.Application.Interfaces.v1;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bff.Api.Integration.Infrastructure;

public sealed class BffApiFactory : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string?> _previousEnvironment = new();

    public StubCreateWorkoutSagaOrchestrator WorkoutOrchestrator { get; } = new();

    public StubSaveGoalSagaOrchestrator GoalOrchestrator { get; } = new();

    public BffApiFactory()
    {
        SetEnvironmentVariable("TRAINING_SERVICE_URL", "http://localhost:5031/");
        SetEnvironmentVariable("ANALYTICS_SERVICE_URL", "http://localhost:5299/");
        SetEnvironmentVariable("NOTIFICATION_SERVICE_URL", "http://localhost:5213/");
        SetEnvironmentVariable("AUTH_SERVICE_URL", "http://localhost:5120/");
        SetEnvironmentVariable("AI_CHAT_SERVICE_URL", "http://localhost:5214/");
        SetEnvironmentVariable("TRAINING_SERVICE_GRPC_URL", "http://localhost:7031");
        SetEnvironmentVariable("ANALYTICS_SERVICE_GRPC_URL", "http://localhost:7299");
        SetEnvironmentVariable("NOTIFICATION_SERVICE_GRPC_URL", "http://localhost:7213");
    }

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
                ["CreateWorkoutSaga:StepTimeoutSeconds"] = "5",
                ["CreateWorkoutSaga:AnalyticsRequired"] = "false",
                ["CreateWorkoutSaga:GoalsRequired"] = "false",
                ["CreateWorkoutSaga:NotificationRequired"] = "false",
                ["SaveGoalSaga:StepTimeoutSeconds"] = "5",
                ["SaveGoalSaga:NotificationRequired"] = "false",
                ["DownstreamServices:TrainingGrpcUrl"] = "http://localhost:7031",
                ["DownstreamServices:AnalyticsGrpcUrl"] = "http://localhost:7299",
                ["DownstreamServices:NotificationGrpcUrl"] = "http://localhost:7213"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ICreateWorkoutSagaOrchestrator>();
            services.RemoveAll<ISaveGoalSagaOrchestrator>();

            services.AddSingleton(WorkoutOrchestrator);
            services.AddScoped<ICreateWorkoutSagaOrchestrator>(sp => sp.GetRequiredService<StubCreateWorkoutSagaOrchestrator>());

            services.AddSingleton(GoalOrchestrator);
            services.AddScoped<ISaveGoalSagaOrchestrator>(sp => sp.GetRequiredService<StubSaveGoalSagaOrchestrator>());

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            services.AddAuthorization();
        });
    }

    public HttpClient CreateAuthenticatedClient(Guid? userId = null)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, "true");
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeaderName, (userId ?? Guid.NewGuid()).ToString());
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var item in _previousEnvironment)
            {
                Environment.SetEnvironmentVariable(item.Key, item.Value);
            }
        }

        base.Dispose(disposing);
    }

    private void SetEnvironmentVariable(string name, string value)
    {
        if (!_previousEnvironment.ContainsKey(name))
        {
            _previousEnvironment[name] = Environment.GetEnvironmentVariable(name);
        }

        Environment.SetEnvironmentVariable(name, value);
    }
}