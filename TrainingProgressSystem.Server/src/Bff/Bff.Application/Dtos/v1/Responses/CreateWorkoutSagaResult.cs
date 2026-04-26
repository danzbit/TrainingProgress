namespace Bff.Application.Dtos.v1.Responses;

public sealed record CreateWorkoutSagaResult(
    Guid? WorkoutId,
    string? Error = null
);
