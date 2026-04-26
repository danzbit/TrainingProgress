using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Api.Grpc.v1;
using NotificationService.Application.Interfaces.v1;
using Shared.Grpc.Contracts;

namespace NotificationService.Api.Tests.Grpc.v1;

[TestFixture]
public class NotificationSyncGrpcServiceTests
{
    private Mock<INotificationSyncService> _syncServiceMock = null!;
    private NotificationSyncGrpcService _grpcService = null!;

    [SetUp]
    public void SetUp()
    {
        _syncServiceMock = new Mock<INotificationSyncService>(MockBehavior.Strict);
        var logger = Mock.Of<ILogger<NotificationSyncGrpcService>>();
        _grpcService = new NotificationSyncGrpcService(_syncServiceMock.Object, logger);
    }

    [Test]
    public async Task ResetRemindersForWorkout_WhenUserIdIsInvalid_ReturnsFailureAndSkipsServiceCall()
    {
        var request = new ResetRemindersForWorkoutGrpcRequest
        {
            UserId = "invalid-guid",
            WorkoutId = Guid.NewGuid().ToString()
        };

        var context = CreateServerCallContext(CancellationToken.None);

        var result = await _grpcService.ResetRemindersForWorkout(request, context);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo("Invalid UserId format."));
        _syncServiceMock.Verify(
            service => service.ResetRemindersForWorkoutAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ResetRemindersForWorkout_WhenUserIdIsValid_CallsServiceAndReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var cancellationToken = new CancellationTokenSource().Token;
        var request = new ResetRemindersForWorkoutGrpcRequest
        {
            UserId = userId.ToString(),
            WorkoutId = Guid.NewGuid().ToString()
        };

        var context = CreateServerCallContext(cancellationToken);

        _syncServiceMock
            .Setup(service => service.ResetRemindersForWorkoutAsync(userId, It.Is<CancellationToken>(ct => ct == cancellationToken)))
            .Returns(Task.CompletedTask);

        var result = await _grpcService.ResetRemindersForWorkout(request, context);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Error, Is.Empty);
        _syncServiceMock.Verify(
            service => service.ResetRemindersForWorkoutAsync(userId, It.Is<CancellationToken>(ct => ct == cancellationToken)),
            Times.Once);
    }

    [Test]
    public async Task ScheduleRemindersForGoal_WhenIdsAreInvalid_ReturnsFailureAndSkipsServiceCall()
    {
        var request = new ScheduleRemindersForGoalGrpcRequest
        {
            UserId = "invalid-user",
            GoalId = "invalid-goal"
        };

        var context = CreateServerCallContext(CancellationToken.None);

        var result = await _grpcService.ScheduleRemindersForGoal(request, context);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo("Invalid UserId or GoalId format."));
        _syncServiceMock.Verify(
            service => service.ScheduleRemindersForGoalAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ScheduleRemindersForGoal_WhenIdsAreValid_CallsServiceAndReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var goalId = Guid.NewGuid();
        var cancellationToken = new CancellationTokenSource().Token;
        var request = new ScheduleRemindersForGoalGrpcRequest
        {
            UserId = userId.ToString(),
            GoalId = goalId.ToString()
        };

        var context = CreateServerCallContext(cancellationToken);

        _syncServiceMock
            .Setup(service => service.ScheduleRemindersForGoalAsync(userId, goalId, It.Is<CancellationToken>(ct => ct == cancellationToken)))
            .Returns(Task.CompletedTask);

        var result = await _grpcService.ScheduleRemindersForGoal(request, context);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Error, Is.Empty);
        _syncServiceMock.Verify(
            service => service.ScheduleRemindersForGoalAsync(userId, goalId, It.Is<CancellationToken>(ct => ct == cancellationToken)),
            Times.Once);
    }

    private static ServerCallContext CreateServerCallContext(CancellationToken cancellationToken)
    {
        return new TestServerCallContext(cancellationToken);
    }

    private sealed class TestServerCallContext(CancellationToken cancellationToken) : ServerCallContext
    {
        private readonly Metadata _requestHeaders = new();
        private readonly Metadata _responseTrailers = new();

        protected override string MethodCore => "test/notification-sync";

        protected override string HostCore => "localhost";

        protected override string PeerCore => "127.0.0.1";

        protected override DateTime DeadlineCore => DateTime.UtcNow.AddMinutes(1);

        protected override Metadata RequestHeadersCore => _requestHeaders;

        protected override CancellationToken CancellationTokenCore => cancellationToken;

        protected override Metadata ResponseTrailersCore => _responseTrailers;

        protected override Status StatusCore { get; set; }

        protected override WriteOptions? WriteOptionsCore { get; set; }

        protected override AuthContext AuthContextCore =>
            new(string.Empty, new Dictionary<string, List<AuthProperty>>());

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
        {
            throw new NotSupportedException();
        }

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            return Task.CompletedTask;
        }
    }
}
