using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Grpc.Contracts;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Api.Grpc.v1;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;
using TrainingService.Domain.Enums;

namespace TrainingService.Api.Tests.Grpc.v1;

[TestFixture]
public class TrainingSyncGrpcServiceTests
{
    private Mock<IWorkoutService> _workoutServiceMock = null!;
    private Mock<IGoalService> _goalServiceMock = null!;
    private Mock<ILogger<TrainingSyncGrpcService>> _loggerMock = null!;
    private TrainingSyncGrpcService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _workoutServiceMock = new Mock<IWorkoutService>(MockBehavior.Strict);
        _goalServiceMock = new Mock<IGoalService>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<TrainingSyncGrpcService>>();
        _service = new TrainingSyncGrpcService(_workoutServiceMock.Object, _goalServiceMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task CreateWorkout_WhenIdsAreInvalid_ReturnsFailureAndDoesNotCallService()
    {
        var request = new CreateWorkoutGrpcRequest
        {
            UserId = "invalid",
            WorkoutTypeId = "invalid",
            Date = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        var response = await _service.CreateWorkout(request, CreateContext());

        Assert.That(response.IsSuccess, Is.False);
        Assert.That(response.Error, Is.EqualTo("Invalid userId or workoutTypeId."));
        _workoutServiceMock.Verify(service => service.CreateWorkoutAsync(It.IsAny<CreateWorkoutRequest>()), Times.Never);
    }

    [Test]
    public async Task CreateWorkout_WhenServiceSucceeds_ReturnsWorkoutId()
    {
        var userId = Guid.NewGuid();
        var workoutTypeId = Guid.NewGuid();
        var createdWorkoutId = Guid.NewGuid();

        _workoutServiceMock
            .Setup(service => service.CreateWorkoutAsync(It.IsAny<CreateWorkoutRequest>()))
            .ReturnsAsync(ResultOfT<CreateWorkoutResponse>.Success(new CreateWorkoutResponse(createdWorkoutId)));

        var request = new CreateWorkoutGrpcRequest
        {
            UserId = userId.ToString(),
            WorkoutTypeId = workoutTypeId.ToString(),
            Date = Timestamp.FromDateTime(DateTime.UtcNow),
            DurationMin = 60,
            HasDurationMin = true,
            Notes = "session",
            Exercises =
            {
                new CreateWorkoutExerciseGrpcRequest
                {
                    ExerciseTypeId = Guid.NewGuid().ToString(),
                    Sets = 3,
                    Reps = 10
                }
            }
        };

        var response = await _service.CreateWorkout(request, CreateContext());

        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.WorkoutId, Is.EqualTo(createdWorkoutId.ToString()));
        _workoutServiceMock.Verify(service => service.CreateWorkoutAsync(It.Is<CreateWorkoutRequest>(r => r.UserId == userId && r.WorkoutTypeId == workoutTypeId)), Times.Once);
    }

    [Test]
    public async Task SaveGoal_WhenUserIdIsInvalid_ReturnsFailure()
    {
        var request = new SaveGoalGrpcRequest
        {
            UserId = "invalid",
            Name = "Goal"
        };

        var response = await _service.SaveGoal(request, CreateContext());

        Assert.That(response.IsSuccess, Is.False);
        Assert.That(response.Error, Is.EqualTo("Invalid userId."));
        _goalServiceMock.Verify(service => service.CreateGoalAsync(It.IsAny<CreateGoalRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task SaveGoal_WhenMetricTypeIsInvalid_ReturnsFailure()
    {
        var request = new SaveGoalGrpcRequest
        {
            UserId = Guid.NewGuid().ToString(),
            MetricType = 999,
            PeriodType = (int)GoalPeriodType.Weekly,
            Name = "Goal",
            Description = "Desc",
            TargetValue = 5,
            StartDate = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        var response = await _service.SaveGoal(request, CreateContext());

        Assert.That(response.IsSuccess, Is.False);
        Assert.That(response.Error, Is.EqualTo("Invalid metricType."));
    }

    [Test]
    public async Task SaveGoal_WhenServiceSucceeds_ReturnsGoalId()
    {
        var goalId = Guid.NewGuid();

        _goalServiceMock
            .Setup(service => service.CreateGoalAsync(It.IsAny<CreateGoalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<Guid>.Success(goalId));

        var request = new SaveGoalGrpcRequest
        {
            UserId = Guid.NewGuid().ToString(),
            Name = "Goal",
            Description = "Desc",
            MetricType = (int)GoalMetricType.WorkoutCount,
            PeriodType = (int)GoalPeriodType.Weekly,
            TargetValue = 3,
            StartDate = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        var response = await _service.SaveGoal(request, CreateContext());

        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.GoalId, Is.EqualTo(goalId.ToString()));
        _goalServiceMock.Verify(service => service.CreateGoalAsync(It.IsAny<CreateGoalRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteWorkout_WhenWorkoutIdInvalid_ReturnsFailure()
    {
        var request = new DeleteWorkoutGrpcRequest { WorkoutId = "bad" };

        var response = await _service.DeleteWorkout(request, CreateContext());

        Assert.That(response.IsSuccess, Is.False);
        Assert.That(response.Error, Is.EqualTo("Invalid workoutId."));
        _workoutServiceMock.Verify(service => service.DeleteWorkoutAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task DeleteGoal_WhenServiceFails_ReturnsFailure()
    {
        _goalServiceMock
            .Setup(service => service.DeleteGoalAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error(ErrorCode.UnexpectedError, "failed")));

        var request = new DeleteGoalGrpcRequest { GoalId = Guid.NewGuid().ToString() };

        var response = await _service.DeleteGoal(request, CreateContext());

        Assert.That(response.IsSuccess, Is.False);
        Assert.That(response.Error, Is.EqualTo("failed"));
    }

    [Test]
    public async Task UpdateGoalsForWorkout_WhenServiceSucceeds_ReturnsSuccess()
    {
        _goalServiceMock
            .Setup(service => service.UpdateGoalsForWorkoutAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var request = new UpdateGoalsForWorkoutGrpcRequest
        {
            UserId = Guid.NewGuid().ToString(),
            WorkoutId = Guid.NewGuid().ToString()
        };

        var response = await _service.UpdateGoalsForWorkout(request, CreateContext());

        Assert.That(response.IsSuccess, Is.True);
    }

    [Test]
    public async Task RecalculateProgressForGoal_WhenInvalidIds_ReturnsFailure()
    {
        var request = new RecalculateProgressForGoalGrpcRequest
        {
            UserId = "bad",
            GoalId = "bad"
        };

        var response = await _service.RecalculateProgressForGoal(request, CreateContext());

        Assert.That(response.IsSuccess, Is.False);
        Assert.That(response.Error, Is.EqualTo("Invalid userId or goalId."));
        _goalServiceMock.Verify(service => service.RecalculateProgressForGoalAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task RecalculateProgressForGoal_WhenServiceFails_ReturnsFailure()
    {
        _goalServiceMock
            .Setup(service => service.RecalculateProgressForGoalAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error(ErrorCode.UnexpectedError, "cannot recalc")));

        var request = new RecalculateProgressForGoalGrpcRequest
        {
            UserId = Guid.NewGuid().ToString(),
            GoalId = Guid.NewGuid().ToString()
        };

        var response = await _service.RecalculateProgressForGoal(request, CreateContext());

        Assert.That(response.IsSuccess, Is.False);
        Assert.That(response.Error, Is.EqualTo("cannot recalc"));
    }

    private static ServerCallContext CreateContext(CancellationToken cancellationToken = default)
    {
        return new TestServerCallContext(cancellationToken);
    }

    private sealed class TestServerCallContext(CancellationToken cancellationToken) : ServerCallContext
    {
        protected override string MethodCore => "test";
        protected override string HostCore => "localhost";
        protected override string PeerCore => "test-peer";
        protected override DateTime DeadlineCore => DateTime.MaxValue;
        protected override Metadata RequestHeadersCore => new();
        protected override CancellationToken CancellationTokenCore => cancellationToken;
        protected override Metadata ResponseTrailersCore { get; } = new();
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore { get; } =
            new("anonymous", new Dictionary<string, List<AuthProperty>>());

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
