using AnalyticsService.Application.Interfaces.v1;
using AnalyticsService.Application.Dtos.v1.Responses;
using AnalyticsService.Domain.Interfaces.v1;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.Auth;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace AnalyticsService.Application.Services.v1;

public class WorkoutAnalyticsService(
	IWorkoutRepository workoutRepository,
	IAnalyticsSnapshotService analyticsSnapshotService,
	ICurrentUser currentUser,
	ILogger<WorkoutAnalyticsService> logger) : IWorkoutAnalyticsService
{
	public async Task<ResultOfT<WorkoutSummaryResponse>> GetSummaryAsync(CancellationToken ct = default)
	{
		logger.LogInformation("Getting workout analytics summary for current user");

		var userIdResult = currentUser.GetCurrentUserId();

		if (userIdResult.IsFailure)
		{
			logger.LogWarning("Failed to get workout summary: invalid current user");
			return ResultOfT<WorkoutSummaryResponse>.Failure(userIdResult.Error);
		}

		var snapshotResult = await analyticsSnapshotService.GetSnapshotAsync(userIdResult.Value, ct);
		if (snapshotResult.IsFailure)
		{
			logger.LogWarning("Failed to get cached workout summary snapshot for user {UserId}", userIdResult.Value);
			return ResultOfT<WorkoutSummaryResponse>.Failure(snapshotResult.Error);
		}

		var response = snapshotResult.Value.Summary;

		logger.LogInformation("Workout summary generated for user {UserId}", userIdResult.Value);

		return ResultOfT<WorkoutSummaryResponse>.Success(response);
	}

	public async Task<ResultOfT<IReadOnlyList<WorkoutDailyTrendPointResponse>>> GetDailyTrendAsync(int days = 7,
		CancellationToken ct = default)
	{
		logger.LogInformation("Getting daily workout trend for current user. Days: {Days}", days);

		var userIdResult = currentUser.GetCurrentUserId();

		if (userIdResult.IsFailure)
		{
			logger.LogWarning("Failed to get daily trend: invalid current user");
			return ResultOfT<IReadOnlyList<WorkoutDailyTrendPointResponse>>.Failure(userIdResult.Error);
		}

		if (days == 7)
		{
			var snapshotResult = await analyticsSnapshotService.GetSnapshotAsync(userIdResult.Value, ct);
			if (snapshotResult.IsFailure)
			{
				logger.LogWarning("Failed to get cached daily trend snapshot for user {UserId}", userIdResult.Value);
				return ResultOfT<IReadOnlyList<WorkoutDailyTrendPointResponse>>.Failure(snapshotResult.Error);
			}

			return ResultOfT<IReadOnlyList<WorkoutDailyTrendPointResponse>>
				.Success(snapshotResult.Value.DailyTrendLast7Days);
		}

		var endDate = DateTime.UtcNow.Date;
		var startDate = endDate.AddDays(-(days - 1));

		var workoutsResult = await workoutRepository.GetDailyTrendByPeriodAsync(userIdResult.Value, startDate,
			endDate.AddDays(1), ct);

		if (workoutsResult.IsFailure)
		{
			logger.LogWarning("Failed to get daily workout trend from repository for user {UserId}", userIdResult.Value);
			return ResultOfT<IReadOnlyList<WorkoutDailyTrendPointResponse>>.Failure(workoutsResult.Error);
		}

		var response = workoutsResult.Value
			.Select(point => new WorkoutDailyTrendPointResponse
			{
				Date = point.Date,
				WorkoutsCount = point.WorkoutsCount,
				DurationMin = point.DurationMin
			})
			.ToList();

		logger.LogInformation("Daily workout trend generated for user {UserId}. Points count: {Count}",
			userIdResult.Value, response.Count);

		return ResultOfT<IReadOnlyList<WorkoutDailyTrendPointResponse>>.Success(response);
	}

	public async Task<ResultOfT<IReadOnlyList<WorkoutCountByTypeResponse>>> GetCountByTypeAsync(DateTime? from = null,
		DateTime? to = null, CancellationToken ct = default)
	{
		logger.LogInformation("Getting workout counts by type for current user. From: {From}, To: {To}", from, to);

		var userIdResult = currentUser.GetCurrentUserId();

		if (userIdResult.IsFailure)
		{
			logger.LogWarning("Failed to get workout counts by type: invalid current user");
			return ResultOfT<IReadOnlyList<WorkoutCountByTypeResponse>>.Failure(userIdResult.Error);
		}

		DateTime fromDate;
		DateTime toDateExclusive;

		if (from.HasValue ^ to.HasValue)
		{
			return ResultOfT<IReadOnlyList<WorkoutCountByTypeResponse>>.Failure(
				new Error(ErrorCode.ValidationFailed, "Both from and to should be provided together."));
		}

		if (!from.HasValue && !to.HasValue)
		{
			var snapshotResult = await analyticsSnapshotService.GetSnapshotAsync(userIdResult.Value, ct);
			if (snapshotResult.IsFailure)
			{
				logger.LogWarning("Failed to get cached count-by-type snapshot for user {UserId}", userIdResult.Value);
				return ResultOfT<IReadOnlyList<WorkoutCountByTypeResponse>>.Failure(snapshotResult.Error);
			}

			return ResultOfT<IReadOnlyList<WorkoutCountByTypeResponse>>
				.Success(snapshotResult.Value.CountByTypeLast7Days);
		}
		else
		{
			var fromValue = from!.Value.Date;
			var toValue = to!.Value.Date;

			if (fromValue > toValue)
			{
				return ResultOfT<IReadOnlyList<WorkoutCountByTypeResponse>>.Failure(
					new Error(ErrorCode.ValidationFailed, "from must be less than or equal to to."));
			}

			fromDate = fromValue;
			toDateExclusive = toValue.AddDays(1);
		}

		var countsResult = await workoutRepository.GetCountByTypeAsync(userIdResult.Value, fromDate, toDateExclusive, ct);

		if (countsResult.IsFailure)
		{
			logger.LogWarning("Failed to get workout counts by type from repository for user {UserId}",
				userIdResult.Value);
			return ResultOfT<IReadOnlyList<WorkoutCountByTypeResponse>>.Failure(countsResult.Error);
		}

		var response = countsResult.Value
			.Select(item => new WorkoutCountByTypeResponse
			{
				WorkoutTypeId = item.WorkoutTypeId,
				WorkoutTypeName = item.WorkoutTypeName,
				WorkoutsCount = item.WorkoutsCount
			})
			.ToList();

		logger.LogInformation(
			"Workout counts by type generated for user {UserId}. Types count: {Count}. From: {FromDate}, ToExclusive: {ToDateExclusive}",
			userIdResult.Value,
			response.Count,
			fromDate,
			toDateExclusive);

		return ResultOfT<IReadOnlyList<WorkoutCountByTypeResponse>>.Success(response);
	}

	public async Task<ResultOfT<WorkoutStatisticsOverviewResponse>> GetStatisticsOverviewAsync(
		CancellationToken ct = default)
	{
		logger.LogInformation("Getting workout statistics overview for current user");

		var userIdResult = currentUser.GetCurrentUserId();

		if (userIdResult.IsFailure)
		{
			logger.LogWarning("Failed to get workout statistics overview: invalid current user");
			return ResultOfT<WorkoutStatisticsOverviewResponse>.Failure(userIdResult.Error);
		}

		var snapshotResult = await analyticsSnapshotService.GetSnapshotAsync(userIdResult.Value, ct);
		if (snapshotResult.IsFailure)
		{
			logger.LogWarning("Failed to get cached statistics overview snapshot for user {UserId}",
				userIdResult.Value);
			return ResultOfT<WorkoutStatisticsOverviewResponse>.Failure(snapshotResult.Error);
		}

		var response = snapshotResult.Value.StatisticsOverview;

		logger.LogInformation("Workout statistics overview generated for user {UserId}", userIdResult.Value);

		return ResultOfT<WorkoutStatisticsOverviewResponse>.Success(response);
	}
}