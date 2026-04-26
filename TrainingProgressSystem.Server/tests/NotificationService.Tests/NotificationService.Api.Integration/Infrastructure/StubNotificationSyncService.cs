using NotificationService.Application.Interfaces.v1;

namespace NotificationService.Api.Integration.Infrastructure;

public sealed class StubNotificationSyncService : INotificationSyncService
{
    public Guid? LastUserId { get; private set; }
    public Guid? LastGoalId { get; private set; }
    public CancellationToken? LastCancellationToken { get; private set; }
    public int ScheduleRemindersForGoalCallCount { get; private set; }
    public int ResetRemindersForWorkoutCallCount { get; private set; }

    public Func<Guid, Guid, CancellationToken, Task>? ScheduleRemindersForGoalHandler { get; set; }
    public Func<Guid, CancellationToken, Task>? ResetRemindersForWorkoutHandler { get; set; }

    public Task ScheduleRemindersForGoalAsync(Guid userId, Guid goalId, CancellationToken ct = default)
    {
        LastUserId = userId;
        LastGoalId = goalId;
        LastCancellationToken = ct;
        ScheduleRemindersForGoalCallCount++;

        if (ScheduleRemindersForGoalHandler is not null)
        {
            return ScheduleRemindersForGoalHandler(userId, goalId, ct);
        }

        return Task.CompletedTask;
    }

    public Task ResetRemindersForWorkoutAsync(Guid userId, CancellationToken ct = default)
    {
        LastUserId = userId;
        LastCancellationToken = ct;
        ResetRemindersForWorkoutCallCount++;

        if (ResetRemindersForWorkoutHandler is not null)
        {
            return ResetRemindersForWorkoutHandler(userId, ct);
        }

        return Task.CompletedTask;
    }

    public void Reset()
    {
        LastUserId = null;
        LastGoalId = null;
        LastCancellationToken = null;
        ScheduleRemindersForGoalCallCount = 0;
        ResetRemindersForWorkoutCallCount = 0;
        ScheduleRemindersForGoalHandler = null;
        ResetRemindersForWorkoutHandler = null;
    }
}
