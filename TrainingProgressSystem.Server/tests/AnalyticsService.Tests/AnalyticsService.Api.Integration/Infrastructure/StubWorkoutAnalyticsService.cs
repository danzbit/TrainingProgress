using AnalyticsService.Application.Dtos.v1.Responses;
using AnalyticsService.Application.Interfaces.v1;
using Shared.Kernal.Results;

namespace AnalyticsService.Api.Integration.Infrastructure;

public sealed class StubWorkoutAnalyticsService : IWorkoutAnalyticsService
{
    public Func<CancellationToken, Task<ResultOfT<WorkoutSummaryResponse>>> SummaryHandler { get; set; } = default!;

    public Func<int, CancellationToken, Task<ResultOfT<IReadOnlyList<WorkoutDailyTrendPointResponse>>>> DailyTrendHandler
    {
        get;
        set;
    } = default!;

    public Func<DateTime?, DateTime?, CancellationToken, Task<ResultOfT<IReadOnlyList<WorkoutCountByTypeResponse>>>>
        CountByTypeHandler { get; set; } = default!;

    public Func<CancellationToken, Task<ResultOfT<WorkoutStatisticsOverviewResponse>>> StatisticsOverviewHandler
    {
        get;
        set;
    } = default!;

    public StubWorkoutAnalyticsService()
    {
        Reset();
    }

    public int? LastDailyTrendDays { get; private set; }
    public DateTime? LastFrom { get; private set; }
    public DateTime? LastTo { get; private set; }

    public Task<ResultOfT<WorkoutSummaryResponse>> GetSummaryAsync(CancellationToken ct = default)
        => SummaryHandler(ct);

    public Task<ResultOfT<IReadOnlyList<WorkoutDailyTrendPointResponse>>> GetDailyTrendAsync(int days = 7,
        CancellationToken ct = default)
    {
        LastDailyTrendDays = days;
        return DailyTrendHandler(days, ct);
    }

    public Task<ResultOfT<IReadOnlyList<WorkoutCountByTypeResponse>>> GetCountByTypeAsync(DateTime? from = null,
        DateTime? to = null, CancellationToken ct = default)
    {
        LastFrom = from;
        LastTo = to;
        return CountByTypeHandler(from, to, ct);
    }

    public Task<ResultOfT<WorkoutStatisticsOverviewResponse>> GetStatisticsOverviewAsync(CancellationToken ct = default)
        => StatisticsOverviewHandler(ct);

    public void Reset()
    {
        SummaryHandler = _ => Task.FromResult(ResultOfT<WorkoutSummaryResponse>.Success(new WorkoutSummaryResponse
        {
            AmountPerWeek = 4,
            WeekDurationMin = 180,
            AmountThisMonth = 16,
            MonthlyTimeMin = 720
        }));

        DailyTrendHandler = (_, _) => Task.FromResult(ResultOfT<IReadOnlyList<WorkoutDailyTrendPointResponse>>.Success(
            new List<WorkoutDailyTrendPointResponse>
            {
                new() { Date = new DateTime(2026, 4, 20), WorkoutsCount = 1, DurationMin = 45 },
                new() { Date = new DateTime(2026, 4, 21), WorkoutsCount = 2, DurationMin = 90 }
            }));

        CountByTypeHandler = (_, _, _) => Task.FromResult(
            ResultOfT<IReadOnlyList<WorkoutCountByTypeResponse>>.Success(new List<WorkoutCountByTypeResponse>
            {
                new() { WorkoutTypeId = Guid.NewGuid(), WorkoutTypeName = "Strength", WorkoutsCount = 3 },
                new() { WorkoutTypeId = Guid.NewGuid(), WorkoutTypeName = "Cardio", WorkoutsCount = 2 }
            }));

        StatisticsOverviewHandler = _ => Task.FromResult(
            ResultOfT<WorkoutStatisticsOverviewResponse>.Success(new WorkoutStatisticsOverviewResponse
            {
                TotalAchievedGoals = 7,
                TotalTrainingHours = 32.5,
                TotalWorkoutsCompleted = 54,
                WorkoutsThisWeek = 4
            }));

        LastDailyTrendDays = null;
        LastFrom = null;
        LastTo = null;
    }
}
