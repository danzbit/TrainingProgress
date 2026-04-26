namespace AnalyticsService.Application.Interfaces.v1;

using Shared.Kernal.Results;

public interface IAnalyticsSyncService
{
    Task<ResultOfT<bool>> RecalculateForWorkoutAsync(Guid userId, Guid workoutId, CancellationToken ct = default);
}
