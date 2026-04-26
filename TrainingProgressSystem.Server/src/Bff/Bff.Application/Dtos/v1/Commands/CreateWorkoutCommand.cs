namespace Bff.Application.Dtos.v1.Commands;

public sealed record CreateWorkoutCommand(
    Guid UserId,
    DateTime Date,
    Guid WorkoutTypeId,
    int? DurationMin,
    string? Notes,
    IReadOnlyList<CreateWorkoutExerciseCommand>? Exercises
);
