namespace Bff.Application.Dtos.v1.Commands;

public sealed record SaveGoalCommand(
    Guid UserId,
    string Name,
    string Description,
    int MetricType,
    int PeriodType,
    int TargetValue,
    DateTime StartDate,
    DateTime? EndDate
);
