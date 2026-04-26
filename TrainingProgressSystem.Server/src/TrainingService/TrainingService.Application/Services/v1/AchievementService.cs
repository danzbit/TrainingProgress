using Microsoft.Extensions.Logging;
using Shared.Abstractions.Auth;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Interfaces.v1;

namespace TrainingService.Application.Services.v1;

public class AchievementService(
    IAchievementRepository achievementRepository,
    ICurrentUser currentUser,
    ILogger<AchievementService> logger) : IAchievementService
{
    private static readonly TimeSpan ShareLinkExpiration = TimeSpan.FromDays(30);

    public async Task<ResultOfT<ShareProgressResponse>> ShareProgressAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Processing share progress request");

        var userIdResult = currentUser.GetCurrentUserId();
        if (userIdResult.IsFailure)
        {
            logger.LogWarning("Failed to share progress: invalid current user");
            return ResultOfT<ShareProgressResponse>.Failure(userIdResult.Error);
        }

        var userId = userIdResult.Value;

        var existingResult = await achievementRepository.GetActiveSharedAchievementByUserIdAsync(userId, ct);
        if (existingResult.IsFailure)
        {
            logger.LogWarning("Failed to retrieve existing share link for user {UserId}", userId);
            return ResultOfT<ShareProgressResponse>.Failure(existingResult.Error);
        }

        if (existingResult.Value is not null)
        {
            logger.LogInformation("Returning existing share link for user {UserId}", userId);
            return ResultOfT<ShareProgressResponse>.Success(
                new ShareProgressResponse(existingResult.Value.PublicUrlKey));
        }

        var achievement = new Achievement
        {
            Title = "Progress Share",
            Description = "Shared fitness progress snapshot",
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
        };

        var addAchievementResult = await achievementRepository.AddAchievementAsync(achievement, ct);
        if (addAchievementResult.IsFailure)
        {
            logger.LogWarning("Failed to create achievement for user {UserId}", userId);
            return ResultOfT<ShareProgressResponse>.Failure(addAchievementResult.Error);
        }

        var publicUrlKey = Guid.NewGuid().ToString("N");

        var sharedAchievement = new SharedAchievement
        {
            AchievementId = achievement.Id,
            PublicUrlKey = publicUrlKey,
            CreatedAt = DateTime.UtcNow,
            Expiration = DateTime.UtcNow.Add(ShareLinkExpiration),
        };

        var addSharedResult = await achievementRepository.AddSharedAchievementAsync(sharedAchievement, ct);
        if (addSharedResult.IsFailure)
        {
            logger.LogWarning("Failed to create shared achievement for user {UserId}", userId);
            return ResultOfT<ShareProgressResponse>.Failure(addSharedResult.Error);
        }

        logger.LogInformation("Share link created for user {UserId} with key {PublicUrlKey}", userId, publicUrlKey);

        return ResultOfT<ShareProgressResponse>.Success(new ShareProgressResponse(publicUrlKey));
    }

    public async Task<ResultOfT<SharedProgressResponse>> GetSharedProgressAsync(string publicUrlKey, CancellationToken ct = default)
    {
        logger.LogInformation("Retrieving shared progress for key {PublicUrlKey}", publicUrlKey);

        var result = await achievementRepository.GetActiveSharedAchievementByKeyAsync(publicUrlKey, ct);

        if (result.IsFailure)
        {
            logger.LogWarning("Failed to retrieve shared progress for key {PublicUrlKey}", publicUrlKey);
            return ResultOfT<SharedProgressResponse>.Failure(result.Error);
        }

        if (result.Value is null)
        {
            logger.LogWarning("Shared progress not found or expired for key {PublicUrlKey}", publicUrlKey);
            return ResultOfT<SharedProgressResponse>.Failure(Error.EntityNotFound);
        }

        var achievement = result.Value.Achievement;

        return ResultOfT<SharedProgressResponse>.Success(new SharedProgressResponse(
            achievement.Title,
            achievement.Description,
            result.Value.CreatedAt,
            result.Value.Expiration));
    }
}
