using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Interfaces;

public interface IRemindersRepository
{
    IReadOnlyList<GoalReminder> GetGoalReminders(Guid userId);

    Task<Goal?> GetGoalWithProgressAsync(Guid goalId, Guid userId, CancellationToken ct = default);

    IReadOnlyList<Goal> GetActiveGoalsWithProgress(Guid userId);

    Task UpsertGoalReminderAsync(GoalReminder reminder, CancellationToken ct = default);

    Task RemoveGoalReminderAsync(Guid goalId, CancellationToken ct = default);
}
