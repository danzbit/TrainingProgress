using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces.v1;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Services.v1;

public sealed class NotificationSyncService(
    IRemindersRepository remindersRepository,
    ILogger<NotificationSyncService> logger) : INotificationSyncService
{
    public async Task ScheduleRemindersForGoalAsync(Guid userId, Guid goalId, CancellationToken ct = default)
    {
        var goal = await remindersRepository.GetGoalWithProgressAsync(goalId, userId, ct);

        if (goal is null)
        {
            logger.LogWarning("ScheduleRemindersForGoal: goal {GoalId} not found for user {UserId}", goalId, userId);
            return;
        }

        // If goal is completed or inactive, remove any stale reminder and stop
        if (goal.Status != 0 || goal.Progress?.IsCompleted == true)
        {
            await remindersRepository.RemoveGoalReminderAsync(goalId, ct);
            return;
        }

        var currentValue = goal.Progress?.CurrentValue ?? 0;
        var remaining = Math.Max(0, goal.TargetValue - currentValue);

        var reminder = new GoalReminder
        {
            Id = goalId,
            GoalId = goalId,
            Name = goal.Name,
            MetricType = goal.MetricType,
            PeriodType = goal.PeriodType,
            TargetValue = goal.TargetValue,
            CurrentValue = currentValue,
            Remaining = remaining,
            EndDate = goal.EndDate
        };

        await remindersRepository.UpsertGoalReminderAsync(reminder, ct);

        logger.LogInformation(
            "Scheduled reminder for goal {GoalId}: {Remaining}/{TargetValue} remaining",
            goalId, remaining, goal.TargetValue);
    }

    public async Task ResetRemindersForWorkoutAsync(Guid userId, CancellationToken ct = default)
    {
        var activeGoals = remindersRepository.GetActiveGoalsWithProgress(userId);

        foreach (var goal in activeGoals)
        {
            var currentValue = goal.Progress?.CurrentValue ?? 0;
            var remaining = Math.Max(0, goal.TargetValue - currentValue);

            var reminder = new GoalReminder
            {
                Id = goal.Id,
                GoalId = goal.Id,
                Name = goal.Name,
                MetricType = goal.MetricType,
                PeriodType = goal.PeriodType,
                TargetValue = goal.TargetValue,
                CurrentValue = currentValue,
                Remaining = remaining,
                EndDate = goal.EndDate
            };

            await remindersRepository.UpsertGoalReminderAsync(reminder, ct);
        }

        logger.LogInformation(
            "Reset reminders for user {UserId}: updated {Count} goal reminder(s)",
            userId, activeGoals.Count);
    }
}
