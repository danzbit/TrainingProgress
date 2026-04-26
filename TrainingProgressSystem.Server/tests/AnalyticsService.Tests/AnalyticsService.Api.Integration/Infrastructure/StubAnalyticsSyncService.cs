using AnalyticsService.Application.Interfaces.v1;
using Shared.Kernal.Results;

namespace AnalyticsService.Api.Integration.Infrastructure;

public sealed class StubAnalyticsSyncService : IAnalyticsSyncService
{
    public Guid? LastUserId { get; private set; }
    public Guid? LastWorkoutId { get; private set; }
    public CancellationToken? LastCancellationToken { get; private set; }
    public int RecalculateCallCount { get; private set; }

    public Func<Guid, Guid, CancellationToken, Task<ResultOfT<bool>>>? RecalculateHandler { get; set; }

    public Task<ResultOfT<bool>> RecalculateForWorkoutAsync(Guid userId, Guid workoutId, CancellationToken ct = default)
    {
        LastUserId = userId;
        LastWorkoutId = workoutId;
        LastCancellationToken = ct;
        RecalculateCallCount++;

        if (RecalculateHandler is not null)
        {
            return RecalculateHandler(userId, workoutId, ct);
        }

        return Task.FromResult(ResultOfT<bool>.Success(true));
    }

    public void Reset()
    {
        LastUserId = null;
        LastWorkoutId = null;
        LastCancellationToken = null;
        RecalculateCallCount = 0;
        RecalculateHandler = null;
    }
}
