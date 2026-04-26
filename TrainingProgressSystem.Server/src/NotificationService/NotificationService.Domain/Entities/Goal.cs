using Shared.Kernal.Models;

namespace NotificationService.Domain.Entities;

public class Goal : BaseEntity
{
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public int MetricType { get; set; }
    public int PeriodType { get; set; }
    public int TargetValue { get; set; }
    public int Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public GoalProgress? Progress { get; set; }
}