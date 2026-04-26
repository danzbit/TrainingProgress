using Shared.Kernal.Models;

namespace AnalyticsService.Domain.Entities;

public class AnalyticsSnapshot : BaseEntity
{
    public Guid UserId { get; set; }

    public int AmountPerWeek { get; set; }

    public int WeekDurationMin { get; set; }

    public int AmountThisMonth { get; set; }

    public int MonthlyTimeMin { get; set; }

    public int TotalAchievedGoals { get; set; }

    public int TotalWorkoutsCompleted { get; set; }

    public int WorkoutsThisWeek { get; set; }

    public double TotalTrainingHours { get; set; }

    public string DailyTrendJson { get; set; } = "[]";

    public string CountByTypeJson { get; set; } = "[]";

    public DateTime LastCalculatedAtUtc { get; set; }
}
