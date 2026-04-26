namespace Bff.Application.Dtos.v1.Responses;

public sealed record SaveGoalSagaResult(
    Guid? GoalId,
    string? Error = null
);
