namespace TrainingService.Application.Dtos.v1.Requests;

public record ExerciseRequest(
    Guid? ExerciseId,
    Guid ExerciseTypeId,
    int Sets,
    int Reps,
    int? DurationSec,
    decimal? WeightKg
);