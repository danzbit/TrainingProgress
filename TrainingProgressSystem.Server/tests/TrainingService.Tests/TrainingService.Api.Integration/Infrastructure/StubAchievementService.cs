using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;

namespace TrainingService.Api.Integration.Infrastructure;

public sealed class StubAchievementService : IAchievementService
{
    public Func<CancellationToken, Task<ResultOfT<ShareProgressResponse>>> ShareProgressHandler { get; set; } = default!;
    public Func<string, CancellationToken, Task<ResultOfT<SharedProgressResponse>>> GetSharedProgressHandler { get; set; } = default!;

    public StubAchievementService() => Reset();

    public Task<ResultOfT<ShareProgressResponse>> ShareProgressAsync(CancellationToken ct = default)
        => ShareProgressHandler(ct);

    public Task<ResultOfT<SharedProgressResponse>> GetSharedProgressAsync(string publicUrlKey, CancellationToken ct = default)
        => GetSharedProgressHandler(publicUrlKey, ct);

    public void Reset()
    {
        ShareProgressHandler = _ => Task.FromResult(ResultOfT<ShareProgressResponse>.Success(
            new ShareProgressResponse("test-public-key-123")));

        GetSharedProgressHandler = (key, _) => Task.FromResult(ResultOfT<SharedProgressResponse>.Success(
            new SharedProgressResponse("My Progress", "Weekly summary", DateTime.UtcNow.AddDays(-1), null)));
    }
}
