namespace Bff.Application.Options.v1;

public sealed class CreateWorkoutSagaOptions
{
    public const string SectionName = "CreateWorkoutSaga";

    public int StepTimeoutSeconds { get; set; } = 5;

    public bool AnalyticsRequired { get; set; }

    public bool GoalsRequired { get; set; }

    public bool NotificationRequired { get; set; }

    public bool CompensateOnCriticalFailure { get; set; } = true;
}
