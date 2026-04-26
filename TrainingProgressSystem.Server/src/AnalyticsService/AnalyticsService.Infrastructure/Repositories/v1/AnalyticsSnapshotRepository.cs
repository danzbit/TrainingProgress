using AnalyticsService.Domain.Entities;
using AnalyticsService.Domain.Interfaces.v1;
using AnalyticsService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Kernal.Results;

namespace AnalyticsService.Infrastructure.Repositories.v1;

public sealed class AnalyticsSnapshotRepository(
    AnalyticsServiceDbContext dbContext,
    ILogger<AnalyticsSnapshotRepository> logger) : IAnalyticsSnapshotRepository
{
    public async Task<ResultOfT<AnalyticsSnapshot?>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var snapshot = await dbContext.Set<AnalyticsSnapshot>()
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.UserId == userId, ct);

        return ResultOfT<AnalyticsSnapshot?>.Success(snapshot);
    }

    public async Task<Result> UpsertAsync(AnalyticsSnapshot snapshot, CancellationToken ct = default)
    {
        var existing = await dbContext.Set<AnalyticsSnapshot>()
            .FirstOrDefaultAsync(entity => entity.UserId == snapshot.UserId, ct);

        if (existing is null)
        {
            await dbContext.Set<AnalyticsSnapshot>().AddAsync(snapshot, ct);
        }
        else
        {
            existing.AmountPerWeek = snapshot.AmountPerWeek;
            existing.WeekDurationMin = snapshot.WeekDurationMin;
            existing.AmountThisMonth = snapshot.AmountThisMonth;
            existing.MonthlyTimeMin = snapshot.MonthlyTimeMin;
            existing.TotalAchievedGoals = snapshot.TotalAchievedGoals;
            existing.TotalWorkoutsCompleted = snapshot.TotalWorkoutsCompleted;
            existing.WorkoutsThisWeek = snapshot.WorkoutsThisWeek;
            existing.TotalTrainingHours = snapshot.TotalTrainingHours;
            existing.DailyTrendJson = snapshot.DailyTrendJson;
            existing.CountByTypeJson = snapshot.CountByTypeJson;
            existing.LastCalculatedAtUtc = snapshot.LastCalculatedAtUtc;
        }

        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("Analytics snapshot persisted for user {UserId}", snapshot.UserId);

        return Result.Success();
    }
}
