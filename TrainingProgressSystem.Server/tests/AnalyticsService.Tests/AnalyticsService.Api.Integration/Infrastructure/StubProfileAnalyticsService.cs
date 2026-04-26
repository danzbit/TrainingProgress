using AnalyticsService.Application.Dtos.v1.Responses;
using AnalyticsService.Application.Interfaces.v1;
using Shared.Kernal.Results;

namespace AnalyticsService.Api.Integration.Infrastructure;

public sealed class StubProfileAnalyticsService : IProfileAnalyticsService
{
    public Func<CancellationToken, Task<ResultOfT<ProfileAnalyticsResponse>>> ProfileHandler { get; set; } = default!;

    public StubProfileAnalyticsService()
    {
        Reset();
    }

    public Task<ResultOfT<ProfileAnalyticsResponse>> GetProfileAnalyticsAsync(CancellationToken ct = default)
        => ProfileHandler(ct);

    public void Reset()
    {
        ProfileHandler = _ => Task.FromResult(ResultOfT<ProfileAnalyticsResponse>.Success(new ProfileAnalyticsResponse
        {
            TotalWorkoutsCompleted = 54,
            TotalHoursTrained = 32.5,
            GoalsAchieved = 7,
            WorkoutsThisWeek = 4
        }));
    }
}
