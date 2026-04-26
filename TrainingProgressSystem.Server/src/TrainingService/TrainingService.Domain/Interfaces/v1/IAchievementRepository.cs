using Shared.Kernal.Results;
using TrainingService.Domain.Entities;

namespace TrainingService.Domain.Interfaces.v1;

public interface IAchievementRepository
{
    Task<Result> AddAchievementAsync(Achievement achievement, CancellationToken ct = default);

    Task<ResultOfT<SharedAchievement?>> GetActiveSharedAchievementByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<Result> AddSharedAchievementAsync(SharedAchievement sharedAchievement, CancellationToken ct = default);

    Task<ResultOfT<SharedAchievement?>> GetActiveSharedAchievementByKeyAsync(string publicUrlKey, CancellationToken ct = default);
}
