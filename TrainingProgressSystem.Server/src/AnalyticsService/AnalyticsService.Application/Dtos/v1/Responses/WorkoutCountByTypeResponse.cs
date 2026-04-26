namespace AnalyticsService.Application.Dtos.v1.Responses;

public class WorkoutCountByTypeResponse
{
    public Guid WorkoutTypeId { get; set; }

    public string WorkoutTypeName { get; set; } = string.Empty;

    public int WorkoutsCount { get; set; }
}
