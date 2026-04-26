using AnalyticsService.Application.Interfaces.v1;
using Microsoft.Extensions.Logging;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace AnalyticsService.Application.Services.v1;

public sealed class AnalyticsSyncService(
    IAnalyticsSnapshotService analyticsSnapshotService,
    ILogger<AnalyticsSyncService> logger) : IAnalyticsSyncService
{
    public async Task<ResultOfT<bool>> RecalculateForWorkoutAsync(Guid userId, Guid workoutId,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "Recalculating analytics snapshots for user {UserId} triggered by workout {WorkoutId}",
            userId,
            workoutId);

        var refreshResult = await analyticsSnapshotService.RefreshSnapshotAsync(userId, ct);
        if (refreshResult.IsFailure)
        {
            return Failure(refreshResult.Error, "Failed to refresh analytics snapshot.");
        }

        logger.LogInformation(
            "Analytics recalculation finished for user {UserId} triggered by workout {WorkoutId}",
            userId,
            workoutId);

        return ResultOfT<bool>.Success(true);
    }

    private ResultOfT<bool> Failure(Error error, string fallbackMessage)
    {
        var description = string.IsNullOrWhiteSpace(error.Description) ? fallbackMessage : error.Description;
        logger.LogWarning(
            "Analytics sync recalculation failed: {ErrorDescription}. ErrorCode: {ErrorCode}",
            description,
            error.Code);

        return ResultOfT<bool>.Failure(new Error(error.Code, description));
    }
}
