namespace Shared.Kernal.Errors;

public static class BffApplicationErrors
{
    public static Error CompensationWorkoutIdMissing() =>
        new(ErrorCode.ValidationFailed, "Cannot compensate create-workout step because workout id is missing.");

    public static Error CompensationGoalIdMissing() =>
        new(ErrorCode.ValidationFailed, "Cannot compensate save-goal step because goal id is missing.");

    public static Error SagaCriticalStepFailed(string? message) =>
        new(ErrorCode.SagaStepFailed, string.IsNullOrWhiteSpace(message) ? "A required saga step failed." : message);
}
