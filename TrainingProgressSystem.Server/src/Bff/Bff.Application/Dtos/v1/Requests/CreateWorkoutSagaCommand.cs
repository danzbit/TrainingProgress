namespace Bff.Application.Dtos.v1.Requests;

public sealed record CreateWorkoutSagaCommand(
    DateTime Date,
    Guid WorkoutTypeId,
    int? DurationMin,
    string? Notes,
    IReadOnlyList<CreateWorkoutExerciseSagaCommand>? Exercises,
    Guid? CorrelationId
);
