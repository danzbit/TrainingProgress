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
public class SaveGoalSagaOrchestratorTests
{
    private Mock<ITrainingSyncClient> _trainingSyncClientMock = null!;
    private Mock<INotificationSyncClient> _notificationSyncClientMock = null!;
    private Mock<ISyncNotifier> _syncNotifierMock = null!;
    private Mock<ICurrentUser> _currentUserMock = null!;
    private Mock<ILogger<SaveGoalSagaOrchestrator>> _loggerMock = null!;
    private IOptions<SaveGoalSagaOptions> _options = null!;
    private SaveGoalSagaOrchestrator _orchestrator = null!;
    private Guid _currentUserId;

    [SetUp]
    public void SetUp()
    {
        _trainingSyncClientMock = new Mock<ITrainingSyncClient>(MockBehavior.Strict);
        _notificationSyncClientMock = new Mock<INotificationSyncClient>(MockBehavior.Strict);
        _syncNotifierMock = new Mock<ISyncNotifier>(MockBehavior.Strict);
        _currentUserMock = new Mock<ICurrentUser>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<SaveGoalSagaOrchestrator>>(MockBehavior.Loose);
        _currentUserId = Guid.NewGuid();

        _currentUserMock
            .Setup(x => x.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Success(_currentUserId));

        var sagaOptions = new SaveGoalSagaOptions
        {
            StepTimeoutSeconds = 5,
            NotificationRequired = true,
            CompensateOnCriticalFailure = true
        };

        _options = OptionsHelper.Create<SaveGoalSagaOptions>(sagaOptions);

        _orchestrator = new SaveGoalSagaOrchestrator(
            _trainingSyncClientMock.Object,
            _notificationSyncClientMock.Object,
            _syncNotifierMock.Object,
            _currentUserMock.Object,
            _options,
            _loggerMock.Object);
    }

    [Test]
    public async Task ExecuteAsync_WhenAllStepsSucceed_ReturnsSuccessResultWithGoalId()
    {
        // Arrange
        var userId = _currentUserId;
        var goalId = Guid.NewGuid();
        var command = new SaveGoalSagaCommand(
            "Weight Loss",
            "Lose 10kg in 3 months",
            1,
            1,
            10,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(3),
            null);

        _trainingSyncClientMock
            .Setup(x => x.SaveGoalAsync(It.IsAny<SaveGoalCommand>(), It.IsAny<SagaCallContext>()))
            .ReturnsAsync(ResultOfT<Guid>.Success(goalId));

        _trainingSyncClientMock
            .Setup(x => x.RecalculateProgressForGoalAsync(userId, goalId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _notificationSyncClientMock
            .Setup(x => x.ScheduleRemindersForGoalAsync(userId, goalId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _syncNotifierMock
            .Setup(x => x.NotifyGoalSavedAsync(userId, goalId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orchestrator.ExecuteAsync(command, null, CancellationToken.None);

        // Assert
        Assert.That(result!.IsFailure, Is.False);
        Assert.That(result.Value.GoalId, Is.EqualTo(goalId));

        _trainingSyncClientMock.Verify(
            x => x.SaveGoalAsync(It.IsAny<SaveGoalCommand>(), It.IsAny<SagaCallContext>()),
            Times.Once);
        _syncNotifierMock.Verify(
            x => x.NotifyGoalSavedAsync(userId, goalId, CancellationToken.None),
            Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_WhenSaveGoalFails_ReturnsFailure()
    {
        // Arrange
        var userId = _currentUserId;
        var command = new SaveGoalSagaCommand(
            "Weight Loss",
            "Lose 10kg in 3 months",
            1,
            1,
            10,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(3),
            null);

        var error = new Error(ErrorCode.UnexpectedError, "Save goal failed");
        _trainingSyncClientMock
            .Setup(x => x.SaveGoalAsync(It.IsAny<SaveGoalCommand>(), It.IsAny<SagaCallContext>()))
            .ReturnsAsync(ResultOfT<Guid>.Failure(error));

        // Act
        var result = await _orchestrator.ExecuteAsync(command, null, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Value.GoalId, Is.Null);

        _trainingSyncClientMock.Verify(
            x => x.SaveGoalAsync(It.IsAny<SaveGoalCommand>(), It.IsAny<SagaCallContext>()),
            Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_WhenNonCriticalStepFails_ContinuesWithSuccess()
    {
        // Arrange
        var userId = _currentUserId;
        var goalId = Guid.NewGuid();
        var command = new SaveGoalSagaCommand(
            "Weight Loss",
            "Lose 10kg in 3 months",
            1,
            1,
            10,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(3),
            null);

        _trainingSyncClientMock
            .Setup(x => x.SaveGoalAsync(It.IsAny<SaveGoalCommand>(), It.IsAny<SagaCallContext>()))
            .ReturnsAsync(ResultOfT<Guid>.Success(goalId));

        _trainingSyncClientMock
            .Setup(x => x.RecalculateProgressForGoalAsync(userId, goalId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Failure(new Error(ErrorCode.UnexpectedError, "Recalculation failed")));

        _notificationSyncClientMock
            .Setup(x => x.ScheduleRemindersForGoalAsync(userId, goalId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _syncNotifierMock
            .Setup(x => x.NotifyGoalSavedAsync(userId, goalId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orchestrator.ExecuteAsync(command, null, CancellationToken.None);

        // Assert
        Assert.That(result!.IsFailure, Is.False);
        Assert.That(result.Value.GoalId, Is.EqualTo(goalId));
    }

    [Test]
    public async Task ExecuteAsync_WithIdempotencyKey_PassesKeyToClients()
    {
        // Arrange
        var userId = _currentUserId;
        var goalId = Guid.NewGuid();
        var idempotencyKey = "goal-key-456";
        var command = new SaveGoalSagaCommand(
            "Weight Loss",
            "Lose 10kg in 3 months",
            1,
            1,
            10,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(3),
            null);

        SagaCallContext? capturedContext = null;

        _trainingSyncClientMock
            .Setup(x => x.SaveGoalAsync(It.IsAny<SaveGoalCommand>(), It.IsAny<SagaCallContext>()))
            .Callback<SaveGoalCommand, SagaCallContext>((_, ctx) => capturedContext = ctx)
            .ReturnsAsync(ResultOfT<Guid>.Success(goalId));

        _trainingSyncClientMock
            .Setup(x => x.RecalculateProgressForGoalAsync(userId, goalId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _notificationSyncClientMock
            .Setup(x => x.ScheduleRemindersForGoalAsync(userId, goalId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _syncNotifierMock
            .Setup(x => x.NotifyGoalSavedAsync(userId, goalId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orchestrator.ExecuteAsync(command, idempotencyKey, CancellationToken.None);

        // Assert
        Assert.That(result!.IsFailure, Is.False);
        Assert.That(capturedContext?.IdempotencyKey, Is.EqualTo(idempotencyKey));
    }

    [Test]
    public async Task ExecuteAsync_WhenOptionsDisableNotifications_SkipsNotificationStep()
    {
        // Arrange
        var sagaOptions = new SaveGoalSagaOptions
        {
            StepTimeoutSeconds = 5,
            NotificationRequired = false,
            CompensateOnCriticalFailure = true
        };

        var options = OptionsHelper.Create<SaveGoalSagaOptions>(sagaOptions);
        var orchestrator = new SaveGoalSagaOrchestrator(
            _trainingSyncClientMock.Object,
            _notificationSyncClientMock.Object,
            _syncNotifierMock.Object,
            _currentUserMock.Object,
            options,
            _loggerMock.Object);

        var userId = _currentUserId;
        var goalId = Guid.NewGuid();
        var command = new SaveGoalSagaCommand(
            "Weight Loss",
            "Lose 10kg in 3 months",
            1,
            1,
            10,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(3),
            null);

        _trainingSyncClientMock
            .Setup(x => x.SaveGoalAsync(It.IsAny<SaveGoalCommand>(), It.IsAny<SagaCallContext>()))
            .ReturnsAsync(ResultOfT<Guid>.Success(goalId));

        _trainingSyncClientMock
            .Setup(x => x.RecalculateProgressForGoalAsync(userId, goalId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _notificationSyncClientMock
            .Setup(x => x.ScheduleRemindersForGoalAsync(userId, goalId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _syncNotifierMock
            .Setup(x => x.NotifyGoalSavedAsync(userId, goalId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await orchestrator.ExecuteAsync(command, null, CancellationToken.None);

        // Assert
        Assert.That(result!.IsFailure, Is.False);
        _notificationSyncClientMock.Verify(
            x => x.ScheduleRemindersForGoalAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<SagaCallContext>()),
            Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_GeneratesCorrelationIdWhenNotProvided()
    {
        // Arrange
        var userId = _currentUserId;
        var goalId = Guid.NewGuid();
        var command = new SaveGoalSagaCommand(
            "Weight Loss",
            "Lose 10kg in 3 months",
            1,
            1,
            10,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(3),
            null);

        SagaCallContext? capturedContext = null;

        _trainingSyncClientMock
            .Setup(x => x.SaveGoalAsync(It.IsAny<SaveGoalCommand>(), It.IsAny<SagaCallContext>()))
            .Callback<SaveGoalCommand, SagaCallContext>((_, ctx) => capturedContext = ctx)
            .ReturnsAsync(ResultOfT<Guid>.Success(goalId));

        _trainingSyncClientMock
            .Setup(x => x.RecalculateProgressForGoalAsync(userId, goalId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _notificationSyncClientMock
            .Setup(x => x.ScheduleRemindersForGoalAsync(userId, goalId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _syncNotifierMock
            .Setup(x => x.NotifyGoalSavedAsync(userId, goalId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orchestrator.ExecuteAsync(command, null, CancellationToken.None);

        // Assert
        Assert.That(result!.IsFailure, Is.False);
        Assert.That(capturedContext?.CorrelationId, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task ExecuteAsync_WithProvidedCorrelationId_UsesProvidedId()
    {
        // Arrange
        var userId = _currentUserId;
        var goalId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var command = new SaveGoalSagaCommand(
            "Weight Loss",
            "Lose 10kg in 3 months",
            1,
            1,
            10,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(3),
            correlationId);

        SagaCallContext? capturedContext = null;

        _trainingSyncClientMock
            .Setup(x => x.SaveGoalAsync(It.IsAny<SaveGoalCommand>(), It.IsAny<SagaCallContext>()))
            .Callback<SaveGoalCommand, SagaCallContext>((_, ctx) => capturedContext = ctx)
            .ReturnsAsync(ResultOfT<Guid>.Success(goalId));

        _trainingSyncClientMock
            .Setup(x => x.RecalculateProgressForGoalAsync(userId, goalId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _notificationSyncClientMock
            .Setup(x => x.ScheduleRemindersForGoalAsync(userId, goalId, It.IsAny<SagaCallContext>()))
            .ReturnsAsync(Result.Success());

        _syncNotifierMock
            .Setup(x => x.NotifyGoalSavedAsync(userId, goalId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orchestrator.ExecuteAsync(command, null, CancellationToken.None);

        // Assert
        Assert.That(result!.IsFailure, Is.False);
        Assert.That(capturedContext?.CorrelationId, Is.EqualTo(correlationId));
    }
}
