namespace Bff.Application.Dtos.v1.Commands;

public sealed record CreateWorkoutExerciseCommand(
    Guid? ExerciseId,
    Guid ExerciseTypeId,
    int Sets,
    int Reps,
    int? DurationSec,
    decimal? WeightKg
);
