namespace Bff.Application.Dtos.v1.Requests;

public sealed record CreateWorkoutExerciseSagaCommand(
    Guid? ExerciseId,
    Guid ExerciseTypeId,
    int Sets,
    int Reps,
    int? DurationSec,
    decimal? WeightKg
);
