namespace Bff.Application.Dtos.v1.Responses;

public sealed record SaveGoalSagaResponse(
    Guid? GoalId,
    string? Error = null
);
