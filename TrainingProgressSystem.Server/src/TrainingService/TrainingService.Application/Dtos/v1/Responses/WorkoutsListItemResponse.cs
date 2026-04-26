namespace TrainingService.Application.Dtos.v1.Responses;

public sealed record WorkoutsListItemResponse(
    string WorkoutType,
    int DurationMin,
    int AmountOfExercises,
    DateTime Date
);
