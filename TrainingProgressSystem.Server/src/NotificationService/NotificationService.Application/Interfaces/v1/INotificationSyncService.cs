namespace NotificationService.Application.Interfaces.v1;

public interface INotificationSyncService
{
    Task ScheduleRemindersForGoalAsync(Guid userId, Guid goalId, CancellationToken ct = default);

    Task ResetRemindersForWorkoutAsync(Guid userId, CancellationToken ct = default);
}
