namespace AnalyticsService.Domain.Models;

public class WorkoutCountByType
{
    public Guid WorkoutTypeId { get; set; }

    public string WorkoutTypeName { get; set; } = string.Empty;

    public int WorkoutsCount { get; set; }
}
