using TrainingService.Domain.Enums;

namespace TrainingService.Application.Dtos.v1.Responses;

public sealed record GoalsListItemResponse(
    Guid Id,
    string Name,
    string Description,
    GoalMetricType MetricType,
    GoalPeriodType PeriodType,
    GoalStatus Status,
    int TargetValue,
    DateTime StartDate,
    DateTime? EndDate,
    GoalProgressInfoResponse ProgressInfo
);
