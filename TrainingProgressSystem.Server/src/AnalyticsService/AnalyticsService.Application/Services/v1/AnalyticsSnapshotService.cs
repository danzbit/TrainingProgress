using System.Text.Json;
using AnalyticsService.Application.Dtos.v1.Responses;
using AnalyticsService.Application.Interfaces.v1;
using AnalyticsService.Domain.Entities;
using AnalyticsService.Domain.Interfaces.v1;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.Caching;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace AnalyticsService.Application.Services.v1;

public sealed class AnalyticsSnapshotService(
    IWorkoutRepository workoutRepository,
    IAnalyticsSnapshotRepository analyticsSnapshotRepository,
    ICacheService cacheService,
    ILogger<AnalyticsSnapshotService> logger) : IAnalyticsSnapshotService
{
    private static string CacheKey(Guid userId) => $"analytics:snapshot:{userId}";

    public async Task<ResultOfT<AnalyticsSnapshotData>> GetSnapshotAsync(Guid userId, CancellationToken ct = default)
    {
        var cached = await cacheService.GetAsync<AnalyticsSnapshotData>(CacheKey(userId), ct);
        if (cached is not null)
        {
            return ResultOfT<AnalyticsSnapshotData>.Success(cached);
        }

        var persistedResult = await analyticsSnapshotRepository.GetByUserIdAsync(userId, ct);
        if (persistedResult.IsFailure)
        {
            return ResultOfT<AnalyticsSnapshotData>.Failure(persistedResult.Error);
        }

        if (persistedResult.Value is not null)
        {
            var mapped = MapEntityToSnapshotData(persistedResult.Value);
            await cacheService.SetAsync(CacheKey(userId), mapped, ct: ct);
            return ResultOfT<AnalyticsSnapshotData>.Success(mapped);
        }

        return await RefreshSnapshotAsync(userId, ct);
    }

    public async Task<ResultOfT<AnalyticsSnapshotData>> RefreshSnapshotAsync(Guid userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var currentDay = now.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)now.DayOfWeek;
        var weekStart = now.Date.AddDays(1 - currentDay);
        var weekEnd = weekStart.AddDays(7);

        var monthStart = new DateTime(now.Year, now.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var last7DaysStart = now.Date.AddDays(-6);
        var last7DaysEndExclusive = now.Date.AddDays(1);

        var weekAggregateResult = await workoutRepository.GetAggregateByPeriodAsync(userId, weekStart, weekEnd, ct);
        if (weekAggregateResult.IsFailure)
        {
            return ResultOfT<AnalyticsSnapshotData>.Failure(weekAggregateResult.Error);
        }

        var monthAggregateResult = await workoutRepository.GetAggregateByPeriodAsync(userId, monthStart, monthEnd, ct);
        if (monthAggregateResult.IsFailure)
        {
            return ResultOfT<AnalyticsSnapshotData>.Failure(monthAggregateResult.Error);
        }

        var dailyTrendResult = await workoutRepository.GetDailyTrendByPeriodAsync(userId, last7DaysStart,
            last7DaysEndExclusive, ct);
        if (dailyTrendResult.IsFailure)
        {
            return ResultOfT<AnalyticsSnapshotData>.Failure(dailyTrendResult.Error);
        }

        var countByTypeResult = await workoutRepository.GetCountByTypeAsync(userId, last7DaysStart,
            last7DaysEndExclusive, ct);
        if (countByTypeResult.IsFailure)
        {
            return ResultOfT<AnalyticsSnapshotData>.Failure(countByTypeResult.Error);
        }

        var statisticsOverviewResult = await workoutRepository.GetStatisticsOverviewAsync(userId, weekStart, weekEnd, ct);
        if (statisticsOverviewResult.IsFailure)
        {
            return ResultOfT<AnalyticsSnapshotData>.Failure(statisticsOverviewResult.Error);
        }

        var totalWorkoutsResult = await workoutRepository.GetTotalWorkoutsCountAsync(userId, ct);
        if (totalWorkoutsResult.IsFailure)
        {
            return ResultOfT<AnalyticsSnapshotData>.Failure(totalWorkoutsResult.Error);
        }

        var totalDurationResult = await workoutRepository.GetTotalDurationMinutesAsync(userId, ct);
        if (totalDurationResult.IsFailure)
        {
            return ResultOfT<AnalyticsSnapshotData>.Failure(totalDurationResult.Error);
        }

        var achievedGoalsResult = await workoutRepository.GetAchievedGoalsCountAsync(userId, ct);
        if (achievedGoalsResult.IsFailure)
        {
            return ResultOfT<AnalyticsSnapshotData>.Failure(achievedGoalsResult.Error);
        }

        var summary = new WorkoutSummaryResponse
        {
            AmountPerWeek = weekAggregateResult.Value.WorkoutsCount,
            WeekDurationMin = weekAggregateResult.Value.DurationMin,
            AmountThisMonth = monthAggregateResult.Value.WorkoutsCount,
            MonthlyTimeMin = monthAggregateResult.Value.DurationMin
        };

        var statisticsOverview = new WorkoutStatisticsOverviewResponse
        {
            TotalAchievedGoals = statisticsOverviewResult.Value.TotalAchievedGoals,
            TotalTrainingHours = Math.Round(statisticsOverviewResult.Value.TotalTrainingMinutes / 60d, 2),
            TotalWorkoutsCompleted = statisticsOverviewResult.Value.TotalWorkoutsCompleted,
            WorkoutsThisWeek = statisticsOverviewResult.Value.WorkoutsThisWeek
        };

        var profileAnalytics = new ProfileAnalyticsResponse
        {
            TotalWorkoutsCompleted = totalWorkoutsResult.Value,
            TotalHoursTrained = Math.Round(totalDurationResult.Value / 60.0, 2),
            GoalsAchieved = achievedGoalsResult.Value,
            WorkoutsThisWeek = weekAggregateResult.Value.WorkoutsCount
        };

        var dailyTrend = dailyTrendResult.Value
            .Select(point => new WorkoutDailyTrendPointResponse
            {
                Date = point.Date,
                WorkoutsCount = point.WorkoutsCount,
                DurationMin = point.DurationMin
            })
            .ToList();

        var countByType = countByTypeResult.Value
            .Select(item => new WorkoutCountByTypeResponse
            {
                WorkoutTypeId = item.WorkoutTypeId,
                WorkoutTypeName = item.WorkoutTypeName,
                WorkoutsCount = item.WorkoutsCount
            })
            .ToList();

        var snapshot = new AnalyticsSnapshotData
        {
            Summary = summary,
            StatisticsOverview = statisticsOverview,
            ProfileAnalytics = profileAnalytics,
            DailyTrendLast7Days = dailyTrend,
            CountByTypeLast7Days = countByType,
            LastCalculatedAtUtc = now
        };

        var persistResult = await analyticsSnapshotRepository.UpsertAsync(new AnalyticsSnapshot
        {
            UserId = userId,
            AmountPerWeek = summary.AmountPerWeek,
            WeekDurationMin = summary.WeekDurationMin,
            AmountThisMonth = summary.AmountThisMonth,
            MonthlyTimeMin = summary.MonthlyTimeMin,
            TotalAchievedGoals = statisticsOverview.TotalAchievedGoals,
            TotalWorkoutsCompleted = statisticsOverview.TotalWorkoutsCompleted,
            WorkoutsThisWeek = statisticsOverview.WorkoutsThisWeek,
            TotalTrainingHours = statisticsOverview.TotalTrainingHours,
            DailyTrendJson = JsonSerializer.Serialize(dailyTrend),
            CountByTypeJson = JsonSerializer.Serialize(countByType),
            LastCalculatedAtUtc = now
        }, ct);

        if (persistResult.IsFailure)
        {
            logger.LogWarning("Failed to persist analytics snapshot for user {UserId}", userId);
            return ResultOfT<AnalyticsSnapshotData>.Failure(persistResult.Error);
        }

        await cacheService.SetAsync(CacheKey(userId), snapshot, ct: ct);

        logger.LogInformation("Analytics snapshot refreshed for user {UserId}", userId);

        return ResultOfT<AnalyticsSnapshotData>.Success(snapshot);
    }

    private static AnalyticsSnapshotData MapEntityToSnapshotData(AnalyticsSnapshot snapshot)
    {
        var dailyTrend = JsonSerializer.Deserialize<List<WorkoutDailyTrendPointResponse>>(snapshot.DailyTrendJson) ?? [];
        var countByType = JsonSerializer.Deserialize<List<WorkoutCountByTypeResponse>>(snapshot.CountByTypeJson) ?? [];

        return new AnalyticsSnapshotData
        {
            Summary = new WorkoutSummaryResponse
            {
                AmountPerWeek = snapshot.AmountPerWeek,
                WeekDurationMin = snapshot.WeekDurationMin,
                AmountThisMonth = snapshot.AmountThisMonth,
                MonthlyTimeMin = snapshot.MonthlyTimeMin
            },
            StatisticsOverview = new WorkoutStatisticsOverviewResponse
            {
                TotalAchievedGoals = snapshot.TotalAchievedGoals,
                TotalTrainingHours = snapshot.TotalTrainingHours,
                TotalWorkoutsCompleted = snapshot.TotalWorkoutsCompleted,
                WorkoutsThisWeek = snapshot.WorkoutsThisWeek
            },
            ProfileAnalytics = new ProfileAnalyticsResponse
            {
                TotalWorkoutsCompleted = snapshot.TotalWorkoutsCompleted,
                TotalHoursTrained = snapshot.TotalTrainingHours,
                GoalsAchieved = snapshot.TotalAchievedGoals,
                WorkoutsThisWeek = snapshot.WorkoutsThisWeek
            },
            DailyTrendLast7Days = dailyTrend,
            CountByTypeLast7Days = countByType,
            LastCalculatedAtUtc = snapshot.LastCalculatedAtUtc
        };
    }
}
