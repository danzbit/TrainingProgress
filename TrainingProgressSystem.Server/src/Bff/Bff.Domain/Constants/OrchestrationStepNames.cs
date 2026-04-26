namespace Bff.Domain.Constants;

public static class OrchestrationStepNames
{
    public const string CreateWorkout = "training.create-workout";

    public const string SaveGoal = "training.save-goal";
    
    public const string RecalculateGoalProgress = "training.recalculate-goal-progress";

    public const string RecalculateAnalytics = "analytics.recalculate";

    public const string UpdateGoals = "training.update-goals";

    public const string ResetReminders = "notification.reset-reminders";

    public const string ScheduleGoalReminders = "notification.schedule-goal-reminders";
}
