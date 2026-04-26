using TrainingService.Domain.Enums;

namespace TrainingService.Application.Dtos.v1.Responses;

public sealed record GoalsResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string Description,
    GoalMetricType MetricType,
    GoalPeriodType PeriodType,
    int TargetValue,
    GoalStatus Status,
    DateTime StartDate,
    DateTime? EndDate,
    int CurrentValue,
    double ProgressPercentage,
    bool IsCompleted,
    DateTime? LastCalculatedAt
);
