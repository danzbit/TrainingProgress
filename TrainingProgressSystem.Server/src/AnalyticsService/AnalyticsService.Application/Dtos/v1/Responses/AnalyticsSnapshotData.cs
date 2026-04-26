namespace AnalyticsService.Application.Dtos.v1.Responses;

public sealed class AnalyticsSnapshotData
{
    public WorkoutSummaryResponse Summary { get; init; } = new();

    public WorkoutStatisticsOverviewResponse StatisticsOverview { get; init; } = new();

    public ProfileAnalyticsResponse ProfileAnalytics { get; init; } = new();

    public IReadOnlyList<WorkoutDailyTrendPointResponse> DailyTrendLast7Days { get; init; } = [];

    public IReadOnlyList<WorkoutCountByTypeResponse> CountByTypeLast7Days { get; init; } = [];

    public DateTime LastCalculatedAtUtc { get; init; }
}
