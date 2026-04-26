namespace AnalyticsService.Domain.Models;

public class DailyWorkoutTrendPoint
{
    public DateTime Date { get; set; }

    public int WorkoutsCount { get; set; }

    public int DurationMin { get; set; }
}