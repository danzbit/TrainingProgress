using AnalyticsService.Application.Dtos.v1.Responses;
using Shared.Kernal.Results;

namespace AnalyticsService.Application.Interfaces.v1;

public interface IWorkoutAnalyticsService
{
	Task<ResultOfT<WorkoutSummaryResponse>> GetSummaryAsync(CancellationToken ct = default);

	Task<ResultOfT<IReadOnlyList<WorkoutDailyTrendPointResponse>>> GetDailyTrendAsync(int days = 7,
		CancellationToken ct = default);

	Task<ResultOfT<IReadOnlyList<WorkoutCountByTypeResponse>>> GetCountByTypeAsync(DateTime? from = null,
		DateTime? to = null, CancellationToken ct = default);

	Task<ResultOfT<WorkoutStatisticsOverviewResponse>> GetStatisticsOverviewAsync(CancellationToken ct = default);
}