using AnalyticsService.Api.Integration.Infrastructure;
using Grpc.Net.Client;
using Shared.Grpc.Contracts;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace AnalyticsService.Api.Integration.Grpc.v1;

[TestFixture]
[NonParallelizable]
public class AnalyticsSyncGrpcServiceIntegrationTests
{
    private AnalyticsApiFactory _factory = null!;
    private GrpcChannel _channel = null!;
    private AnalyticsSyncGrpc.AnalyticsSyncGrpcClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new AnalyticsApiFactory();
        _client = _factory.CreateGrpcClient(out _channel);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _channel.Dispose();
        _factory.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        _factory.AnalyticsSyncService.Reset();
    }

    [Test]
    public async Task RecalculateForWorkout_WithValidRequest_CallsServiceAndReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();

        var request = new RecalculateForWorkoutGrpcRequest
        {
            UserId = userId.ToString(),
            WorkoutId = workoutId.ToString()
        };

        var response = await _client.RecalculateForWorkoutAsync(request);

        Assert.That(response.IsSuccess, Is.True);
        Assert.That(_factory.AnalyticsSyncService.RecalculateCallCount, Is.EqualTo(1));
        Assert.That(_factory.AnalyticsSyncService.LastUserId, Is.EqualTo(userId));
        Assert.That(_factory.AnalyticsSyncService.LastWorkoutId, Is.EqualTo(workoutId));
    }

    [Test]
    public async Task RecalculateForWorkout_WithInvalidIds_ReturnsFailureAndSkipsServiceCall()
    {
        var request = new RecalculateForWorkoutGrpcRequest
        {
            UserId = "invalid-guid",
            WorkoutId = "invalid-guid"
        };

        var response = await _client.RecalculateForWorkoutAsync(request);

        Assert.That(response.IsSuccess, Is.False);
        Assert.That(response.Error, Does.Contain("Invalid UserId or WorkoutId format"));
        Assert.That(_factory.AnalyticsSyncService.RecalculateCallCount, Is.EqualTo(0));
    }

    [Test]
    public async Task RecalculateForWorkout_WhenServiceReturnsFailure_ReturnsFailure()
    {
        _factory.AnalyticsSyncService.RecalculateHandler = (_, _, _) =>
            Task.FromResult(ResultOfT<bool>.Failure(new Error(ErrorCode.UnexpectedError, "sync failed")));

        var request = new RecalculateForWorkoutGrpcRequest
        {
            UserId = Guid.NewGuid().ToString(),
            WorkoutId = Guid.NewGuid().ToString()
        };

        var response = await _client.RecalculateForWorkoutAsync(request);

        Assert.That(response.IsSuccess, Is.False);
        Assert.That(response.Error, Is.EqualTo("sync failed"));
    }
}
