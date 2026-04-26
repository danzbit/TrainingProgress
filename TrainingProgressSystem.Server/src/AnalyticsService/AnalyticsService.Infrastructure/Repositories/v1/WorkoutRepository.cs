using AnalyticsService.Domain.Interfaces.v1;
using AnalyticsService.Domain.Models;
using AnalyticsService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Kernal.Results;

namespace AnalyticsService.Infrastructure.Repositories.v1;

public class WorkoutRepository(
    AnalyticsServiceDbContext dbContext,
    ILogger<WorkoutRepository> logger) : IWorkoutRepository
{
    private const int CompletedGoalStatus = 1;

    public async Task<ResultOfT<WorkoutAggregate>> GetAggregateByPeriodAsync(Guid userId, DateTime from, DateTime to,
        CancellationToken ct = default)
    {
        logger.LogInformation("Getting workout aggregate for user {UserId} from {From} to {To}", userId, from, to);

        var aggregate = await dbContext.Workouts
            .AsNoTracking()
            .Where(workout => workout.UserId == userId && workout.Date >= from && workout.Date < to)
            .GroupBy(_ => 1)
            .Select(group => new WorkoutAggregate
            {
                WorkoutsCount = group.Count(),
                DurationMin = group.Sum(workout => workout.DurationMin)
            })
            .FirstOrDefaultAsync(ct);

        logger.LogInformation("Workout aggregate retrieved for user {UserId}", userId);

        return ResultOfT<WorkoutAggregate>.Success(aggregate ?? new WorkoutAggregate());
    }

    public async Task<ResultOfT<IReadOnlyList<DailyWorkoutTrendPoint>>> GetDailyTrendByPeriodAsync(Guid userId,
        DateTime from, DateTime to, CancellationToken ct = default)
    {
        logger.LogInformation("Getting daily workout trend for user {UserId} from {From} to {To}", userId, from, to);

        var trendPoints = await dbContext.Workouts
            .AsNoTracking()
            .Where(workout => workout.UserId == userId && workout.Date >= from && workout.Date < to)
            .GroupBy(workout => workout.Date.Date)
            .Select(group => new DailyWorkoutTrendPoint
            {
                Date = group.Key,
                WorkoutsCount = group.Count(),
                DurationMin = group.Sum(workout => workout.DurationMin)
            })
            .OrderBy(point => point.Date)
            .ToListAsync(ct);

        logger.LogInformation("Daily workout trend retrieved for user {UserId}. Points count: {Count}", userId,
            trendPoints.Count);

        return ResultOfT<IReadOnlyList<DailyWorkoutTrendPoint>>.Success(trendPoints);
    }

    public async Task<ResultOfT<IReadOnlyList<WorkoutCountByType>>> GetCountByTypeAsync(Guid userId, DateTime from,
        DateTime to, CancellationToken ct = default)
    {
        logger.LogInformation("Getting workout counts by type for user {UserId} from {From} to {To}", userId, from,
            to);

        var countsByType = await dbContext.Workouts
            .AsNoTracking()
            .Where(workout => workout.UserId == userId && workout.Date >= from && workout.Date < to)
            .GroupBy(workout => new { workout.WorkoutTypeId, WorkoutTypeName = workout.WorkoutType.Name })
            .Select(group => new WorkoutCountByType
            {
                WorkoutTypeId = group.Key.WorkoutTypeId,
                WorkoutTypeName = group.Key.WorkoutTypeName,
                WorkoutsCount = group.Count()
            })
            .OrderByDescending(item => item.WorkoutsCount)
            .ThenBy(item => item.WorkoutTypeName)
            .ToListAsync(ct);

        logger.LogInformation("Workout counts by type retrieved for user {UserId}. Types count: {TypesCount}",
            userId, countsByType.Count);

        return ResultOfT<IReadOnlyList<WorkoutCountByType>>.Success(countsByType);
    }

    public async Task<ResultOfT<WorkoutStatisticsOverview>> GetStatisticsOverviewAsync(Guid userId, DateTime weekStart,
        DateTime weekEnd, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Getting workout statistics overview for user {UserId}. WeekStart: {WeekStart}, WeekEnd: {WeekEnd}",
            userId,
            weekStart,
            weekEnd);

        var workoutsQuery = dbContext.Workouts
            .AsNoTracking()
            .Where(workout => workout.UserId == userId);

        var totalWorkoutsCompleted = await workoutsQuery.CountAsync(ct);

        var totalTrainingMinutes = await workoutsQuery
            .SumAsync(workout => (int?)workout.DurationMin, ct) ?? 0;

        var workoutsThisWeek = await workoutsQuery
            .Where(workout => workout.Date >= weekStart && workout.Date < weekEnd)
            .CountAsync(ct);

        var totalAchievedGoals = await dbContext.Goals
            .AsNoTracking()
            .Where(goal => goal.UserId == userId && goal.Status == CompletedGoalStatus)
            .CountAsync(ct);

        var overview = new WorkoutStatisticsOverview
        {
            TotalAchievedGoals = totalAchievedGoals,
            TotalTrainingMinutes = totalTrainingMinutes,
            TotalWorkoutsCompleted = totalWorkoutsCompleted,
            WorkoutsThisWeek = workoutsThisWeek
        };

        logger.LogInformation("Workout statistics overview retrieved for user {UserId}", userId);

        return ResultOfT<WorkoutStatisticsOverview>.Success(overview);
    }

    public async Task<ResultOfT<int>> GetTotalWorkoutsCountAsync(Guid userId, CancellationToken ct = default)
    {
        logger.LogInformation("Getting total workouts count for user {UserId}", userId);

        var count = await dbContext.Workouts
            .AsNoTracking()
            .Where(workout => workout.UserId == userId)
            .CountAsync(ct);

        logger.LogInformation("Total workouts count retrieved for user {UserId}: {Count}", userId, count);

        return ResultOfT<int>.Success(count);
    }

    public async Task<ResultOfT<int>> GetTotalDurationMinutesAsync(Guid userId, CancellationToken ct = default)
    {
        logger.LogInformation("Getting total duration minutes for user {UserId}", userId);

        var totalDuration = await dbContext.Workouts
            .AsNoTracking()
            .Where(workout => workout.UserId == userId)
            .SumAsync(workout => workout.DurationMin, ct);

        logger.LogInformation("Total duration minutes retrieved for user {UserId}: {TotalDuration}", userId, totalDuration);

        return ResultOfT<int>.Success(totalDuration);
    }

    public async Task<ResultOfT<int>> GetAchievedGoalsCountAsync(Guid userId, CancellationToken ct = default)
    {
        logger.LogInformation("Getting achieved goals count for user {UserId}", userId);

        var count = await dbContext.Goals
            .AsNoTracking()
            .Where(goal => goal.UserId == userId && goal.Status == CompletedGoalStatus)
            .CountAsync(ct);

        logger.LogInformation("Achieved goals count retrieved for user {UserId}: {Count}", userId, count);

        return ResultOfT<int>.Success(count);
    }
}