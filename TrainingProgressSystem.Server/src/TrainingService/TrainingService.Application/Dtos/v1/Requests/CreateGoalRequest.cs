using TrainingService.Domain.Enums;

namespace TrainingService.Application.Dtos.v1.Requests;

public sealed record CreateGoalRequest(
    Guid UserId,
    string Name,
    string Description,
    GoalMetricType MetricType,
    GoalPeriodType PeriodType,
    int TargetValue,
    DateTime StartDate,
    DateTime? EndDate
);
