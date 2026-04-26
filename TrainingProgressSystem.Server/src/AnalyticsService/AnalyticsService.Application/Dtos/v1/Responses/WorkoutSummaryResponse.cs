namespace AnalyticsService.Application.Dtos.v1.Responses;

public class WorkoutSummaryResponse
{
    public int AmountPerWeek { get; set; }

    public int WeekDurationMin { get; set; }

    public int AmountThisMonth { get; set; }

    public int MonthlyTimeMin { get; set; }
}