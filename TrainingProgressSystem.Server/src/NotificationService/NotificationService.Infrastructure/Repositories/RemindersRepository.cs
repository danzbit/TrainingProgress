using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Infrastructure.Repositories;

public class RemindersRepository(NotificationServiceDbContext context) : IRemindersRepository
{
    public IReadOnlyList<GoalReminder> GetGoalReminders(Guid userId)
    {
        var goalReminders = context.Goals
            .AsNoTracking()
            .Include(g => g.Progress)
            .Where(g => g.UserId == userId
                && g.Status == 0  // Active
                && (g.Progress == null || g.Progress.IsCompleted == false)  // Not completed
                && (g.TargetValue - (g.Progress == null ? 0 : g.Progress.CurrentValue)) > 0)  // Only include goals with remaining progress
            .OrderByDescending(g => g.Id)
            .Select(g => new GoalReminder
            {
                Id = g.Id,
                GoalId = g.Id,
                Name = g.Name,
                MetricType = g.MetricType,
                PeriodType = g.PeriodType,
                TargetValue = g.TargetValue,
                CurrentValue = g.Progress != null ? g.Progress.CurrentValue : 0,
                Remaining = g.TargetValue - (g.Progress != null ? g.Progress.CurrentValue : 0),
                EndDate = g.EndDate
            })
            .ToList();

        return goalReminders;
    }

    public Task<Goal?> GetGoalWithProgressAsync(Guid goalId, Guid userId, CancellationToken ct = default)
        => context.Goals
            .Include(g => g.Progress)
            .FirstOrDefaultAsync(g => g.Id == goalId && g.UserId == userId, ct);

    public IReadOnlyList<Goal> GetActiveGoalsWithProgress(Guid userId)
        => context.Goals
            .Include(g => g.Progress)
            .Where(g => g.UserId == userId && g.Status == 0 && (g.Progress == null || g.Progress.IsCompleted == false))
            .ToList();

    public async Task UpsertGoalReminderAsync(GoalReminder reminder, CancellationToken ct = default)
    {
        var existing = await context.GoalReminders
            .FirstOrDefaultAsync(r => r.GoalId == reminder.GoalId, ct);

        if (existing is null)
        {
            context.GoalReminders.Add(reminder);
        }
        else
        {
            existing.Name = reminder.Name;
            existing.MetricType = reminder.MetricType;
            existing.PeriodType = reminder.PeriodType;
            existing.TargetValue = reminder.TargetValue;
            existing.CurrentValue = reminder.CurrentValue;
            existing.Remaining = reminder.Remaining;
            existing.EndDate = reminder.EndDate;
        }

        await context.SaveChangesAsync(ct);
    }

    public async Task RemoveGoalReminderAsync(Guid goalId, CancellationToken ct = default)
    {
        var existing = await context.GoalReminders
            .FirstOrDefaultAsync(r => r.GoalId == goalId, ct);

        if (existing is not null)
        {
            context.GoalReminders.Remove(existing);
            await context.SaveChangesAsync(ct);
        }
    }
}