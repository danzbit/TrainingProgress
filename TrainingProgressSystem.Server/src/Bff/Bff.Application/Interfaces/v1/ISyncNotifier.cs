namespace Bff.Application.Interfaces.v1;

public interface ISyncNotifier
{
    Task NotifyWorkoutCreatedAsync(Guid userId, Guid workoutId, CancellationToken ct = default);

    Task NotifyGoalSavedAsync(Guid userId, Guid goalId, CancellationToken ct = default);
}
