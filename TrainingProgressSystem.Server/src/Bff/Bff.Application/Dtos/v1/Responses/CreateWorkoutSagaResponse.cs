namespace Bff.Application.Dtos.v1.Responses;

public sealed record CreateWorkoutSagaResponse(
    Guid? WorkoutId,
    string? Error = null
);
