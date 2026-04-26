using Grpc.Net.Client;
using NotificationService.Api.Integration.Infrastructure;
using Shared.Grpc.Contracts;

namespace NotificationService.Api.Integration.Grpc.v1;

[TestFixture]
[NonParallelizable]
public class NotificationSyncGrpcServiceIntegrationTests
{
    private NotificationServiceApiFactory _factory = null!;
    private GrpcChannel _channel = null!;
    private NotificationSyncGrpc.NotificationSyncGrpcClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new NotificationServiceApiFactory();
        _client = _factory.CreateGrpcClient(out _channel);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _channel?.Dispose();
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }
    }

    [SetUp]
    public void SetUp()
    {
        _factory.NotificationSyncService.Reset();
    }

    [Test]
    public async Task ResetRemindersForWorkout_WithValidRequest_CallsServiceAndReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ResetRemindersForWorkoutGrpcRequest { UserId = userId.ToString(), WorkoutId = "workout-123" };

        // Act
        var response = await _client.ResetRemindersForWorkoutAsync(request);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(_factory.NotificationSyncService.ResetRemindersForWorkoutCallCount, Is.EqualTo(1));
        Assert.That(_factory.NotificationSyncService.LastUserId, Is.EqualTo(userId));
    }

    [Test]
    public async Task ResetRemindersForWorkout_WithInvalidUserId_ReturnsFalseWithError()
    {
        // Arrange
        var request = new ResetRemindersForWorkoutGrpcRequest { UserId = "invalid-guid", WorkoutId = "workout-123" };

        // Act
        var response = await _client.ResetRemindersForWorkoutAsync(request);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccess, Is.False);
        Assert.That(response.Error, Does.Contain("Invalid UserId format"));
    }

    [Test]
    public async Task ScheduleRemindersForGoal_WithValidRequest_CallsServiceAndReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var goalId = Guid.NewGuid();
        var request = new ScheduleRemindersForGoalGrpcRequest 
        { 
            UserId = userId.ToString(), 
            GoalId = goalId.ToString() 
        };

        // Act
        var response = await _client.ScheduleRemindersForGoalAsync(request);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(_factory.NotificationSyncService.ScheduleRemindersForGoalCallCount, Is.EqualTo(1));
        Assert.That(_factory.NotificationSyncService.LastUserId, Is.EqualTo(userId));
        Assert.That(_factory.NotificationSyncService.LastGoalId, Is.EqualTo(goalId));
    }

    [Test]
    public async Task ScheduleRemindersForGoal_WithInvalidUserId_ReturnsFalseWithError()
    {
        // Arrange
        var request = new ScheduleRemindersForGoalGrpcRequest 
        { 
            UserId = "invalid-guid", 
            GoalId = Guid.NewGuid().ToString() 
        };

        // Act
        var response = await _client.ScheduleRemindersForGoalAsync(request);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccess, Is.False);
        Assert.That(response.Error, Does.Contain("Invalid UserId or GoalId format"));
    }

    [Test]
    public async Task ScheduleRemindersForGoal_WithInvalidGoalId_ReturnsFalseWithError()
    {
        // Arrange
        var request = new ScheduleRemindersForGoalGrpcRequest 
        { 
            UserId = Guid.NewGuid().ToString(), 
            GoalId = "invalid-guid" 
        };

        // Act
        var response = await _client.ScheduleRemindersForGoalAsync(request);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccess, Is.False);
        Assert.That(response.Error, Does.Contain("Invalid UserId or GoalId format"));
    }

    [Test]
    public async Task ScheduleRemindersForGoal_WhenServiceThrowsException_StillReturnsSuccessResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var goalId = Guid.NewGuid();
        _factory.NotificationSyncService.ScheduleRemindersForGoalHandler = (_, _, _) =>
            throw new InvalidOperationException("Service error");

        var request = new ScheduleRemindersForGoalGrpcRequest 
        { 
            UserId = userId.ToString(), 
            GoalId = goalId.ToString() 
        };

        // Act & Assert
        Assert.ThrowsAsync<global::Grpc.Core.RpcException>(async () =>
            await _client.ScheduleRemindersForGoalAsync(request));
    }
}
