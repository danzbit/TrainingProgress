using Shared.Kernal.Models;
using TrainingService.Domain.Enums;

namespace TrainingService.Domain.Entities;

public class Goal : BaseEntity
{
    public Guid UserId { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public GoalMetricType MetricType { get; set; }

    public GoalPeriodType PeriodType { get; set; }

    public int TargetValue { get; set; }

    public GoalStatus Status { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public GoalProgress? Progress { get; set; }
}
