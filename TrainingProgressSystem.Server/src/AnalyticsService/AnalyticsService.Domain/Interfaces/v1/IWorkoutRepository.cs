using AnalyticsService.Domain.Models;
using Shared.Kernal.Results;

namespace AnalyticsService.Domain.Interfaces.v1;

public interface IWorkoutRepository
{
    Task<ResultOfT<WorkoutAggregate>> GetAggregateByPeriodAsync(Guid userId, DateTime from, DateTime to,
        CancellationToken ct = default);

    Task<ResultOfT<IReadOnlyList<DailyWorkoutTrendPoint>>> GetDailyTrendByPeriodAsync(Guid userId, DateTime from,
        DateTime to, CancellationToken ct = default);

    Task<ResultOfT<IReadOnlyList<WorkoutCountByType>>> GetCountByTypeAsync(Guid userId, DateTime from, DateTime to,
        CancellationToken ct = default);

    Task<ResultOfT<WorkoutStatisticsOverview>> GetStatisticsOverviewAsync(Guid userId, DateTime weekStart,
        DateTime weekEnd, CancellationToken ct = default);

    Task<ResultOfT<int>> GetTotalWorkoutsCountAsync(Guid userId, CancellationToken ct = default);

    Task<ResultOfT<int>> GetTotalDurationMinutesAsync(Guid userId, CancellationToken ct = default);

    Task<ResultOfT<int>> GetAchievedGoalsCountAsync(Guid userId, CancellationToken ct = default);
}