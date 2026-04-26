using Microsoft.EntityFrameworkCore;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Interfaces.v1;
using TrainingService.Infrastructure.Data;

namespace TrainingService.Infrastructure.Repositories.v1;

public class AchievementRepository(TrainingServiceDbContext dbContext) : IAchievementRepository
{
    public async Task<Result> AddAchievementAsync(Achievement achievement, CancellationToken ct = default)
    {
        try
        {
            await dbContext.Achievements.AddAsync(achievement, ct);
            await dbContext.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(Error.UnexpectedError);
        }
    }

    public async Task<ResultOfT<SharedAchievement?>> GetActiveSharedAchievementByUserIdAsync(
        Guid userId, CancellationToken ct = default)
    {
        try
        {
            var now = DateTime.UtcNow;

            var shared = await dbContext.SharedAchievements
                .AsNoTracking()
                .Include(sa => sa.Achievement)
                .Where(sa => sa.Achievement.UserId == userId &&
                             (sa.Expiration == null || sa.Expiration > now))
                .OrderByDescending(sa => sa.CreatedAt)
                .FirstOrDefaultAsync(ct);

            return ResultOfT<SharedAchievement?>.Success(shared);
        }
        catch (Exception)
        {
            return ResultOfT<SharedAchievement?>.Failure(Error.UnexpectedError);
        }
    }

    public async Task<Result> AddSharedAchievementAsync(SharedAchievement sharedAchievement,
        CancellationToken ct = default)
    {
        try
        {
            await dbContext.SharedAchievements.AddAsync(sharedAchievement, ct);
            await dbContext.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(Error.UnexpectedError);
        }
    }

    public async Task<ResultOfT<SharedAchievement?>> GetActiveSharedAchievementByKeyAsync(
        string publicUrlKey, CancellationToken ct = default)
    {
        try
        {
            var now = DateTime.UtcNow;

            var shared = await dbContext.SharedAchievements
                .AsNoTracking()
                .Include(sa => sa.Achievement)
                .Where(sa => sa.PublicUrlKey == publicUrlKey &&
                             (sa.Expiration == null || sa.Expiration > now))
                .FirstOrDefaultAsync(ct);

            return ResultOfT<SharedAchievement?>.Success(shared);
        }
        catch (Exception)
        {
            return ResultOfT<SharedAchievement?>.Failure(Error.UnexpectedError);
        }
    }
}
