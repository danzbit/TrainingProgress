using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Responses;

namespace TrainingService.Application.Interfaces.v1;

public interface IAchievementService
{
    Task<ResultOfT<ShareProgressResponse>> ShareProgressAsync(CancellationToken ct = default);

    Task<ResultOfT<SharedProgressResponse>> GetSharedProgressAsync(string publicUrlKey, CancellationToken ct = default);
}
