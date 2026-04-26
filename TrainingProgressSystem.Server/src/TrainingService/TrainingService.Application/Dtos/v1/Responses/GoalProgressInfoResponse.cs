namespace TrainingService.Application.Dtos.v1.Responses;

public sealed record GoalProgressInfoResponse(
    int CurrentValue,
    double ProgressPercentage,
    bool IsCompleted,
    DateTime? LastCalculatedAt
);
