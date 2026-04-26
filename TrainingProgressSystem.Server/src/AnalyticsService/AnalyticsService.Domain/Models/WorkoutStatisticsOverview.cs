namespace AnalyticsService.Domain.Models;

public class WorkoutStatisticsOverview
{
    public int TotalAchievedGoals { get; set; }

    public int TotalTrainingMinutes { get; set; }

    public int TotalWorkoutsCompleted { get; set; }

    public int WorkoutsThisWeek { get; set; }
}
