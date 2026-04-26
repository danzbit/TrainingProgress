using Shared.Kernal.Models;

namespace TrainingService.Application.Dtos.v1.Responses;

public record ExerciseResponse(
    Guid Id,
    int Sets,
    int Reps,
    decimal? WeightKg,
    int? DurationSec,
    ExerciseTypeResponse ExerciseType
);