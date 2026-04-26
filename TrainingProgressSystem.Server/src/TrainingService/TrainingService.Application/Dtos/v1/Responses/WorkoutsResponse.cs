using Shared.Kernal.Models;

namespace TrainingService.Application.Dtos.v1.Responses;

public record WorkoutsResponse(
    Guid Id,
    DateTime Date,
    int DurationMin,
    string? Notes,
    WorkoutTypeResponse WorkoutType,
    ICollection<ExerciseResponse> Exercises
);

