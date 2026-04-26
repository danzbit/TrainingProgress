using AnalyticsService.Api.Grpc.v1;
using AnalyticsService.Application.Interfaces.v1;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Grpc.Contracts;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace AnalyticsService.Api.Tests.Grpc.v1;

[TestFixture]
public class AnalyticsSyncGrpcServiceTests
{
    private Mock<IAnalyticsSyncService> _syncServiceMock = null!;
    private AnalyticsSyncGrpcService _grpcService = null!;

    [SetUp]
    public void SetUp()
    {
        _syncServiceMock = new Mock<IAnalyticsSyncService>(MockBehavior.Strict);
        var logger = Mock.Of<ILogger<AnalyticsSyncGrpcService>>();
        _grpcService = new AnalyticsSyncGrpcService(_syncServiceMock.Object, logger);
    }

    [Test]
    public async Task RecalculateForWorkout_WhenIdsAreInvalid_ReturnsFailureAndSkipsServiceCall()
    {
        var request = new RecalculateForWorkoutGrpcRequest
        {
            UserId = "invalid-user-id",
            WorkoutId = "invalid-workout-id"
        };

        var context = CreateServerCallContext(CancellationToken.None);

        var result = await _grpcService.RecalculateForWorkout(request, context);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo("Invalid UserId or WorkoutId format."));
        _syncServiceMock.Verify(
            service => service.RecalculateForWorkoutAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task RecalculateForWorkout_WhenIdsAreValidAndServiceSucceeds_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new RecalculateForWorkoutGrpcRequest
        {
            UserId = userId.ToString(),
            WorkoutId = workoutId.ToString()
        };

        _syncServiceMock
            .Setup(service => service.RecalculateForWorkoutAsync(userId, workoutId,
                It.Is<CancellationToken>(ct => ct == cancellationToken)))
            .ReturnsAsync(ResultOfT<bool>.Success(true));

        var context = CreateServerCallContext(cancellationToken);

        var result = await _grpcService.RecalculateForWorkout(request, context);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Error, Is.Empty);
        _syncServiceMock.Verify(
            service => service.RecalculateForWorkoutAsync(userId, workoutId,
                It.Is<CancellationToken>(ct => ct == cancellationToken)),
            Times.Once);
    }

    [Test]
    public async Task RecalculateForWorkout_WhenServiceFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();

        var request = new RecalculateForWorkoutGrpcRequest
        {
            UserId = userId.ToString(),
            WorkoutId = workoutId.ToString()
        };

        _syncServiceMock
            .Setup(service => service.RecalculateForWorkoutAsync(userId, workoutId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<bool>.Failure(new Error(ErrorCode.UnexpectedError, "refresh failed")));

        var result = await _grpcService.RecalculateForWorkout(request, CreateServerCallContext(CancellationToken.None));

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo("refresh failed"));
    }

    private static ServerCallContext CreateServerCallContext(CancellationToken cancellationToken)
    {
        return new TestServerCallContext(cancellationToken);
    }

    private sealed class TestServerCallContext(CancellationToken cancellationToken) : ServerCallContext
    {
        private readonly Metadata _requestHeaders = new();
        private readonly Metadata _responseTrailers = new();

        protected override string MethodCore => "test/analytics-sync";

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
