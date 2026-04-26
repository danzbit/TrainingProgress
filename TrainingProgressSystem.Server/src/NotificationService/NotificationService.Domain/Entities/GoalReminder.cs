using Shared.Kernal.Models;

namespace NotificationService.Domain.Entities;

public class GoalReminder : BaseEntity
{
    public Guid GoalId { get; set; }
    public required string Name { get; set; }
    public int MetricType { get; set; }
    public int PeriodType { get; set; }
    public int TargetValue { get; set; }
    public int CurrentValue { get; set; }
    public int Remaining { get; set; }
    public DateTime? EndDate { get; set; }
}