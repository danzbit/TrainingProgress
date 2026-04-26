using AnalyticsService.Application.Interfaces.v1;
using AnalyticsService.Application.Dtos.v1.Responses;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.Auth;
using Shared.Kernal.Results;

namespace AnalyticsService.Application.Services.v1;

public class ProfileAnalyticsService(
    IAnalyticsSnapshotService analyticsSnapshotService,
    ICurrentUser currentUser,
    ILogger<ProfileAnalyticsService> logger) : IProfileAnalyticsService
{
    public async Task<ResultOfT<ProfileAnalyticsResponse>> GetProfileAnalyticsAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Getting profile analytics for current user");

        var userIdResult = currentUser.GetCurrentUserId();

        if (userIdResult.IsFailure)
        {
            logger.LogWarning("Failed to get profile analytics: invalid current user");
            return ResultOfT<ProfileAnalyticsResponse>.Failure(userIdResult.Error);
        }

        var userId = userIdResult.Value;

        var snapshotResult = await analyticsSnapshotService.GetSnapshotAsync(userId, ct);
        if (snapshotResult.IsFailure)
        {
            logger.LogWarning("Failed to get cached profile analytics snapshot for user {UserId}", userId);
            return ResultOfT<ProfileAnalyticsResponse>.Failure(snapshotResult.Error);
        }

        var response = snapshotResult.Value.ProfileAnalytics;

        logger.LogInformation("Profile analytics retrieved successfully for user {UserId}", userId);

        return ResultOfT<ProfileAnalyticsResponse>.Success(response);
    }
}