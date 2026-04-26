namespace AnalyticsService.Application.Dtos.v1.Responses;

public class WorkoutDailyTrendPointResponse
{
    public DateTime Date { get; set; }

    public int WorkoutsCount { get; set; }

    public int DurationMin { get; set; }
}