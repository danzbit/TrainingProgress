using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Application.Services.v1;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Tests.Services.v1;

[TestFixture]
public class NotificationSyncServiceTests
{
    private Mock<IRemindersRepository> _repositoryMock = null!;
    private NotificationSyncService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IRemindersRepository>(MockBehavior.Strict);
        var logger = Mock.Of<ILogger<NotificationSyncService>>();
        _service = new NotificationSyncService(_repositoryMock.Object, logger);
    }

    [Test]
    public async Task ScheduleRemindersForGoalAsync_WhenGoalNotFound_DoesNotUpsertOrRemove()
    {
        var userId = Guid.NewGuid();
        var goalId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetGoalWithProgressAsync(goalId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Goal?)null);

        await _service.ScheduleRemindersForGoalAsync(userId, goalId, CancellationToken.None);

        _repositoryMock.Verify(r => r.UpsertGoalReminderAsync(It.IsAny<GoalReminder>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.RemoveGoalReminderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ScheduleRemindersForGoalAsync_WhenGoalIsInactive_RemovesExistingReminder()
    {
        var userId = Guid.NewGuid();
        var goalId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetGoalWithProgressAsync(goalId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Goal
            {
                Id = goalId,
                UserId = userId,
                Name = "Old Goal",
                MetricType = 0,
                PeriodType = 0,
                TargetValue = 10,
                Status = 1
            });

        _repositoryMock
            .Setup(r => r.RemoveGoalReminderAsync(goalId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.ScheduleRemindersForGoalAsync(userId, goalId, CancellationToken.None);

        _repositoryMock.Verify(r => r.RemoveGoalReminderAsync(goalId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.UpsertGoalReminderAsync(It.IsAny<GoalReminder>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ScheduleRemindersForGoalAsync_WhenGoalProgressIsCompleted_RemovesExistingReminder()
    {
        var userId = Guid.NewGuid();
        var goalId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetGoalWithProgressAsync(goalId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Goal
            {
                Id = goalId,
                UserId = userId,
                Name = "Completed Goal",
                MetricType = 0,
                PeriodType = 0,
                TargetValue = 10,
                Status = 0,
                Progress = new GoalProgress { GoalId = goalId, CurrentValue = 10, IsCompleted = true }
            });

        _repositoryMock
            .Setup(r => r.RemoveGoalReminderAsync(goalId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.ScheduleRemindersForGoalAsync(userId, goalId, CancellationToken.None);

        _repositoryMock.Verify(r => r.RemoveGoalReminderAsync(goalId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.UpsertGoalReminderAsync(It.IsAny<GoalReminder>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ScheduleRemindersForGoalAsync_WhenGoalIsActiveAndIncomplete_UpsertsReminderWithCorrectValues()
    {
        var userId = Guid.NewGuid();
        var goalId = Guid.NewGuid();
        var endDate = new DateTime(2026, 5, 31);

        _repositoryMock
            .Setup(r => r.GetGoalWithProgressAsync(goalId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Goal
            {
                Id = goalId,
                UserId = userId,
                Name = "Get Fit",
                MetricType = 0,
                PeriodType = 1,
                TargetValue = 20,
                Status = 0,
                EndDate = endDate,
                Progress = new GoalProgress { GoalId = goalId, CurrentValue = 12, IsCompleted = false }
            });

        GoalReminder? capturedReminder = null;
        _repositoryMock
            .Setup(r => r.UpsertGoalReminderAsync(It.IsAny<GoalReminder>(), It.IsAny<CancellationToken>()))
            .Callback<GoalReminder, CancellationToken>((r, _) => capturedReminder = r)
            .Returns(Task.CompletedTask);

        await _service.ScheduleRemindersForGoalAsync(userId, goalId, CancellationToken.None);

        _repositoryMock.Verify(r => r.UpsertGoalReminderAsync(It.IsAny<GoalReminder>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.That(capturedReminder, Is.Not.Null);
        Assert.That(capturedReminder!.GoalId, Is.EqualTo(goalId));
        Assert.That(capturedReminder.Name, Is.EqualTo("Get Fit"));
        Assert.That(capturedReminder.CurrentValue, Is.EqualTo(12));
        Assert.That(capturedReminder.Remaining, Is.EqualTo(8));
        Assert.That(capturedReminder.EndDate, Is.EqualTo(endDate));
    }

    [Test]
    public async Task ScheduleRemindersForGoalAsync_WhenNoProgress_UsesZeroCurrentValueAndFullTargetAsRemaining()
    {
        var userId = Guid.NewGuid();
        var goalId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetGoalWithProgressAsync(goalId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Goal
            {
                Id = goalId,
                UserId = userId,
                Name = "New Goal",
                MetricType = 0,
                PeriodType = 0,
                TargetValue = 15,
                Status = 0,
                Progress = null
            });

        GoalReminder? capturedReminder = null;
        _repositoryMock
            .Setup(r => r.UpsertGoalReminderAsync(It.IsAny<GoalReminder>(), It.IsAny<CancellationToken>()))
            .Callback<GoalReminder, CancellationToken>((r, _) => capturedReminder = r)
            .Returns(Task.CompletedTask);

        await _service.ScheduleRemindersForGoalAsync(userId, goalId, CancellationToken.None);

        Assert.That(capturedReminder!.CurrentValue, Is.EqualTo(0));
        Assert.That(capturedReminder.Remaining, Is.EqualTo(15));
    }

    [Test]
    public async Task ResetRemindersForWorkoutAsync_WhenNoActiveGoals_DoesNotUpsertAnyReminders()
    {
        var userId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetActiveGoalsWithProgress(userId))
            .Returns([]);

        await _service.ResetRemindersForWorkoutAsync(userId, CancellationToken.None);

        _repositoryMock.Verify(r => r.UpsertGoalReminderAsync(It.IsAny<GoalReminder>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ResetRemindersForWorkoutAsync_UpsertsReminderForEachActiveGoal()
    {
        var userId = Guid.NewGuid();
        var goal1Id = Guid.NewGuid();
        var goal2Id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetActiveGoalsWithProgress(userId))
            .Returns([
                new Goal
                {
                    Id = goal1Id,
                    UserId = userId,
                    Name = "Goal One",
                    MetricType = 0,
                    PeriodType = 0,
                    TargetValue = 10,
                    Status = 0,
                    Progress = new GoalProgress { GoalId = goal1Id, CurrentValue = 3 }
                },
                new Goal
                {
                    Id = goal2Id,
                    UserId = userId,
                    Name = "Goal Two",
                    MetricType = 1,
                    PeriodType = 1,
                    TargetValue = 100,
                    Status = 0,
                    Progress = null
                }
            ]);

        var upsertedReminders = new List<GoalReminder>();
        _repositoryMock
            .Setup(r => r.UpsertGoalReminderAsync(It.IsAny<GoalReminder>(), It.IsAny<CancellationToken>()))
            .Callback<GoalReminder, CancellationToken>((r, _) => upsertedReminders.Add(r))
            .Returns(Task.CompletedTask);

        await _service.ResetRemindersForWorkoutAsync(userId, CancellationToken.None);

        _repositoryMock.Verify(r => r.UpsertGoalReminderAsync(It.IsAny<GoalReminder>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

        var reminder1 = upsertedReminders.Single(r => r.GoalId == goal1Id);
        Assert.That(reminder1.CurrentValue, Is.EqualTo(3));
        Assert.That(reminder1.Remaining, Is.EqualTo(7));

        var reminder2 = upsertedReminders.Single(r => r.GoalId == goal2Id);
        Assert.That(reminder2.CurrentValue, Is.EqualTo(0));
        Assert.That(reminder2.Remaining, Is.EqualTo(100));
    }
}
