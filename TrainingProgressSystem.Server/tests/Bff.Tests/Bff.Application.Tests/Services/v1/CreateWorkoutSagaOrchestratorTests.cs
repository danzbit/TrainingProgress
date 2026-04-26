using Bff.Application.Dtos.v1.Commands;
using Bff.Application.Dtos.v1.Requests;
using Bff.Application.Dtos.v1.Responses;
using Bff.Application.Interfaces.v1;
using Bff.Application.Options.v1;
using Bff.Application.Services.v1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OptionsHelper = Microsoft.Extensions.Options.Options;
using Moq;
using Shared.Abstractions.Auth;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using Shared.Saga.Models;

namespace Bff.Application.Tests.Services.v1;

[TestFixture]
public class CreateWorkoutSagaOrchestratorTests
{
    private Mock<ITrainingSyncClient> _trainingSyncClientMock = null!;
    private Mock<IAnalyticsSyncClient> _analyticsSyncClientMock = null!;
    private Mock<INotificationSyncClient> _notificationSyncClientMock = null!;
    private Mock<ISyncNotifier> _syncNotifierMock = null!;
    private Mock<ICurrentUser> _currentUserMock = null!;
    private Mock<ILogger<CreateWorkoutSagaOrchestrator>> _loggerMock = null!;
    private IOptions<CreateWorkoutSagaOptions> _options = null!;
    private CreateWorkoutSagaOrchestrator _orchestrator = null!;
    private Guid _currentUserId;

    [SetUp]
    public void SetUp()
    {
        _trainingSyncClientMock = new Mock<ITrainingSyncClient>(MockBehavior.Strict);
        _analyticsSyncClientMock = new Mock<IAnalyticsSyncClient>(MockBehavior.Strict);
        _notificationSyncClientMock = new Mock<INotificationSyncClient>(MockBehavior.Strict);
        _syncNotifierMock = new Mock<ISyncNotifier>(MockBehavior.Strict);
        _currentUserMock = new Mock<ICurrentUser>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<CreateWorkoutSagaOrchestrator>>(MockBehavior.Loose);
        _currentUserId = Guid.NewGuid();

        _currentUserMock
            .Setup(x => x.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Success(_currentUserId));

        var sagaOptions = new CreateWorkoutSagaOptions
        {
            StepTimeoutSeconds = 5,
            AnalyticsRequired = true,
            GoalsRequired = true,
            NotificationRequired = true,
            CompensateOnCriticalFailure = true
        };

        _options = OptionsHelper.Create<CreateWorkoutSagaOptions>(sagaOptions);
        
        _orchestrator = new CreateWorkoutSagaOrchestrator(
            _trainingSyncClientMock.Object,
            _analyticsSyncClientMock.Object,
            _notificationSyncClientMock.Object,
            _syncNotifierMock.Object,
            _currentUserMock.Object,
            _options,
            _loggerMock.Object);
    }

    [Test]
    public async Task ExecuteAsync_WhenAllStepsSucceed_ReturnsSuccessResultWithWorkoutId()
    {
        // Arrange
        var userId = _currentUserId;
        var workoutId = Guid.NewGuid();
        var command = new CreateWorkoutSagaCommand(
            DateTime.UtcNow,
            Guid.NewGuid(),
            30,
            "Test workout",
            [],
            null);

        _trainingSyncClientMock
            .Setup(x => x.CreateWorkoutAsync(It.IsAny<CreateWorkoutCommand>(), It.IsAny<SagaCallContext>()))
            .ReturnsAsync(ResultOfT<Guid>.Success(workoutId));

        _analyticsSyncClientMock
            .Setup(x => x.RecalculateForWorkoutAsync(userId, workoutId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _trainingSyncClientMock
            .Setup(x => x.UpdateGoalsForWorkoutAsync(userId, workoutId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _notificationSyncClientMock
            .Setup(x => x.ResetRemindersForWorkoutAsync(userId, workoutId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _syncNotifierMock
            .Setup(x => x.NotifyWorkoutCreatedAsync(userId, workoutId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orchestrator.ExecuteAsync(command, null, CancellationToken.None);

        // Assert
        Assert.That(result!.IsFailure, Is.False);
        Assert.That(result.Value.WorkoutId, Is.EqualTo(workoutId));

        _trainingSyncClientMock.Verify(
            x => x.CreateWorkoutAsync(It.IsAny<CreateWorkoutCommand>(), It.IsAny<SagaCallContext>()),
            Times.Once);
        _syncNotifierMock.Verify(
            x => x.NotifyWorkoutCreatedAsync(userId, workoutId, CancellationToken.None),
            Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_WhenCreateWorkoutFails_ReturnsFailureWithoutCompensation()
    {
        // Arrange
        var userId = _currentUserId;
        var command = new CreateWorkoutSagaCommand(
            DateTime.UtcNow,
            Guid.NewGuid(),
            30,
            "Test workout",
            [],
            null);

        var error = new Error(ErrorCode.UnexpectedError, "Create workout failed");
        _trainingSyncClientMock
            .Setup(x => x.CreateWorkoutAsync(It.IsAny<CreateWorkoutCommand>(), It.IsAny<SagaCallContext>()))
            .ReturnsAsync(ResultOfT<Guid>.Failure(error));

        // Act
        var result = await _orchestrator.ExecuteAsync(command, null, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Value.WorkoutId, Is.Null);

        _trainingSyncClientMock.Verify(
            x => x.CreateWorkoutAsync(It.IsAny<CreateWorkoutCommand>(), It.IsAny<SagaCallContext>()),
            Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_WhenNonCriticalStepFails_ContinuesWithSuccess()
    {
        // Arrange
        var sagaOptions = new CreateWorkoutSagaOptions
        {
            StepTimeoutSeconds = 5,
            AnalyticsRequired = false,
            GoalsRequired = true,
            NotificationRequired = true,
            CompensateOnCriticalFailure = true
        };

        var options = OptionsHelper.Create<CreateWorkoutSagaOptions>(sagaOptions);
        var orchestrator = new CreateWorkoutSagaOrchestrator(
            _trainingSyncClientMock.Object,
            _analyticsSyncClientMock.Object,
            _notificationSyncClientMock.Object,
            _syncNotifierMock.Object,
            _currentUserMock.Object,
            options,
            _loggerMock.Object);

        var userId = _currentUserId;
        var workoutId = Guid.NewGuid();
        var command = new CreateWorkoutSagaCommand(
            DateTime.UtcNow,
            Guid.NewGuid(),
            30,
            "Test workout",
            [],
            null);

        _trainingSyncClientMock
            .Setup(x => x.CreateWorkoutAsync(It.IsAny<CreateWorkoutCommand>(), It.IsAny<SagaCallContext>()))
            .ReturnsAsync(ResultOfT<Guid>.Success(workoutId));

        _analyticsSyncClientMock
            .Setup(x => x.RecalculateForWorkoutAsync(userId, workoutId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Failure(new Error(ErrorCode.UnexpectedError, "Analytics failed")));

        _trainingSyncClientMock
            .Setup(x => x.UpdateGoalsForWorkoutAsync(userId, workoutId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _notificationSyncClientMock
            .Setup(x => x.ResetRemindersForWorkoutAsync(userId, workoutId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _syncNotifierMock
            .Setup(x => x.NotifyWorkoutCreatedAsync(userId, workoutId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await orchestrator.ExecuteAsync(command, null, CancellationToken.None);

        // Assert
        Assert.That(result!.IsFailure, Is.False);
        Assert.That(result.Value.WorkoutId, Is.EqualTo(workoutId));
    }

    [Test]
    public async Task ExecuteAsync_WithIdempotencyKey_PassesKeyToClients()
    {
        // Arrange
        var userId = _currentUserId;
        var workoutId = Guid.NewGuid();
        var idempotencyKey = "test-key-123";
        var command = new CreateWorkoutSagaCommand(
            DateTime.UtcNow,
            Guid.NewGuid(),
            30,
            "Test workout",
            [],
            null);

        SagaCallContext? capturedContext = null;

        _trainingSyncClientMock
            .Setup(x => x.CreateWorkoutAsync(It.IsAny<CreateWorkoutCommand>(), It.IsAny<SagaCallContext>()))
            .Callback<CreateWorkoutCommand, SagaCallContext>((_, ctx) => capturedContext = ctx)
            .ReturnsAsync(ResultOfT<Guid>.Success(workoutId));

        _analyticsSyncClientMock
            .Setup(x => x.RecalculateForWorkoutAsync(userId, workoutId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _trainingSyncClientMock
            .Setup(x => x.UpdateGoalsForWorkoutAsync(userId, workoutId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _notificationSyncClientMock
            .Setup(x => x.ResetRemindersForWorkoutAsync(userId, workoutId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _syncNotifierMock
            .Setup(x => x.NotifyWorkoutCreatedAsync(userId, workoutId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orchestrator.ExecuteAsync(command, idempotencyKey, CancellationToken.None);

        // Assert
        Assert.That(result!.IsFailure, Is.False);
        Assert.That(capturedContext?.IdempotencyKey, Is.EqualTo(idempotencyKey));
    }

    [Test]
    public async Task ExecuteAsync_WhenOptionsDisableAnalytics_SkipsAnalyticsStep()
    {
        // Arrange
        var sagaOptions = new CreateWorkoutSagaOptions
        {
            StepTimeoutSeconds = 5,
            AnalyticsRequired = false,
            GoalsRequired = true,
            NotificationRequired = true,
            CompensateOnCriticalFailure = true
        };

        var options = OptionsHelper.Create<CreateWorkoutSagaOptions>(sagaOptions);
        var orchestrator = new CreateWorkoutSagaOrchestrator(
            _trainingSyncClientMock.Object,
            _analyticsSyncClientMock.Object,
            _notificationSyncClientMock.Object,
            _syncNotifierMock.Object,
            _currentUserMock.Object,
            options,
            _loggerMock.Object);

        var userId = _currentUserId;
        var workoutId = Guid.NewGuid();
        var command = new CreateWorkoutSagaCommand(
            DateTime.UtcNow,
            Guid.NewGuid(),
            30,
            "Test workout",
            [],
            null);

        _trainingSyncClientMock
            .Setup(x => x.CreateWorkoutAsync(It.IsAny<CreateWorkoutCommand>(), It.IsAny<SagaCallContext>()))
            .ReturnsAsync(ResultOfT<Guid>.Success(workoutId));

        _analyticsSyncClientMock
            .Setup(x => x.RecalculateForWorkoutAsync(userId, workoutId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _trainingSyncClientMock
            .Setup(x => x.UpdateGoalsForWorkoutAsync(userId, workoutId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _notificationSyncClientMock
            .Setup(x => x.ResetRemindersForWorkoutAsync(userId, workoutId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _syncNotifierMock
            .Setup(x => x.NotifyWorkoutCreatedAsync(userId, workoutId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await orchestrator.ExecuteAsync(command, null, CancellationToken.None);

        // Assert
        Assert.That(result!.IsFailure, Is.False);
        _analyticsSyncClientMock.Verify(
            x => x.RecalculateForWorkoutAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<SagaCallContext>()),
            Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_GeneratesCorrelationIdWhenNotProvided()
    {
        // Arrange
        var userId = _currentUserId;
        var workoutId = Guid.NewGuid();
        var command = new CreateWorkoutSagaCommand(
            DateTime.UtcNow,
            Guid.NewGuid(),
            30,
            "Test workout",
            [],
            null);

        SagaCallContext? capturedContext = null;

        _trainingSyncClientMock
            .Setup(x => x.CreateWorkoutAsync(It.IsAny<CreateWorkoutCommand>(), It.IsAny<SagaCallContext>()))
            .Callback<CreateWorkoutCommand, SagaCallContext>((_, ctx) => capturedContext = ctx)
            .ReturnsAsync(ResultOfT<Guid>.Success(workoutId));

        _analyticsSyncClientMock
            .Setup(x => x.RecalculateForWorkoutAsync(userId, workoutId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _trainingSyncClientMock
            .Setup(x => x.UpdateGoalsForWorkoutAsync(userId, workoutId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _notificationSyncClientMock
            .Setup(x => x.ResetRemindersForWorkoutAsync(userId, workoutId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _syncNotifierMock
            .Setup(x => x.NotifyWorkoutCreatedAsync(userId, workoutId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orchestrator.ExecuteAsync(command, null, CancellationToken.None);

        // Assert
        Assert.That(result!.IsFailure, Is.False);
        Assert.That(capturedContext?.CorrelationId, Is.Not.EqualTo(Guid.Empty));
    }
}
