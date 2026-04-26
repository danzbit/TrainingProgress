namespace AnalyticsService.Application.Dtos.v1.Responses;

public class WorkoutStatisticsOverviewResponse
{
    public int TotalAchievedGoals { get; set; }

    public double TotalTrainingHours { get; set; }

    public int TotalWorkoutsCompleted { get; set; }

    public int WorkoutsThisWeek { get; set; }
}
