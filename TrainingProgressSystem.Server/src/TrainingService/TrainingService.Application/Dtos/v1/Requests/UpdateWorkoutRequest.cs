namespace TrainingService.Application.Dtos.v1.Requests;

public record UpdateWorkoutRequest(
    Guid WorkoutId,
    Guid UserId,
    DateTime Date,
    Guid WorkoutTypeId,
    int? DurationMin,
    string? Notes,
    List<ExerciseRequest>? Exercises
);