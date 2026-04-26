using System.Text.Json;
using Bff.Api.Integration.Infrastructure;
using Bff.Application.Interfaces.v1;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Bff.Api.Integration.Services;

[TestFixture]
[NonParallelizable]
public class SignalRSyncNotifierIntegrationTests
{
    private BffApiFactory _factory = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new BffApiFactory();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _factory.Dispose();
    }

    [Test]
    public async Task NotifyWorkoutCreatedAsync_WhenUserConnected_PublishesWorkoutCreatedEvent()
    {
        var userId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();
        var received = new TaskCompletionSource<Guid>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var connection = CreateConnection(userId);
        connection.On<JsonElement>("WorkoutCreated", payload =>
        {
            received.TrySetResult(payload.GetProperty("workoutId").GetGuid());
        });

        await connection.StartAsync();

        using var scope = _factory.Services.CreateScope();
        var notifier = scope.ServiceProvider.GetRequiredService<ISyncNotifier>();
        await notifier.NotifyWorkoutCreatedAsync(userId, workoutId);

        Assert.That(await received.Task.WaitAsync(TimeSpan.FromSeconds(5)), Is.EqualTo(workoutId));
    }

    [Test]
    public async Task NotifyGoalSavedAsync_WhenUserConnected_PublishesGoalSavedEvent()
    {
        var userId = Guid.NewGuid();
        var goalId = Guid.NewGuid();
        var received = new TaskCompletionSource<Guid>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var connection = CreateConnection(userId);
        connection.On<JsonElement>("GoalSaved", payload =>
        {
            received.TrySetResult(payload.GetProperty("goalId").GetGuid());
        });

        await connection.StartAsync();

        using var scope = _factory.Services.CreateScope();
        var notifier = scope.ServiceProvider.GetRequiredService<ISyncNotifier>();
        await notifier.NotifyGoalSavedAsync(userId, goalId);

        Assert.That(await received.Task.WaitAsync(TimeSpan.FromSeconds(5)), Is.EqualTo(goalId));
    }

    private HubConnection CreateConnection(Guid userId)
    {
        return new HubConnectionBuilder()
            .WithUrl(new Uri(_factory.Server.BaseAddress!, "/hubs/sync"), options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.Headers[TestAuthHandler.HeaderName] = "true";
                options.Headers[TestAuthHandler.UserIdHeaderName] = userId.ToString();
            })
            .Build();
    }
}