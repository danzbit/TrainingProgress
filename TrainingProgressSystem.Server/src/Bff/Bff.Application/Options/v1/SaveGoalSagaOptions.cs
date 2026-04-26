namespace Bff.Application.Options.v1;

public sealed class SaveGoalSagaOptions
{
    public const string SectionName = "SaveGoalSaga";

    public int StepTimeoutSeconds { get; set; } = 5;

    public bool NotificationRequired { get; set; } = true;

    public bool CompensateOnCriticalFailure { get; set; } = true;
}
