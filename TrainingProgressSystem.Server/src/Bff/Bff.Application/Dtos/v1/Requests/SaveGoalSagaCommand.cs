namespace Bff.Application.Dtos.v1.Requests;

public sealed record SaveGoalSagaCommand(
    string Name,
    string Description,
    int MetricType,
    int PeriodType,
    int TargetValue,
    DateTime StartDate,
    DateTime? EndDate,
    Guid? CorrelationId
);
