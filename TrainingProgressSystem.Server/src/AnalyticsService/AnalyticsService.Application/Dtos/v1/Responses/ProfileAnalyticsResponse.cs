namespace AnalyticsService.Application.Dtos.v1.Responses;

public class ProfileAnalyticsResponse
{
    public int TotalWorkoutsCompleted { get; set; }

    public double TotalHoursTrained { get; set; }

    public int GoalsAchieved { get; set; }

    public int WorkoutsThisWeek { get; set; }
}