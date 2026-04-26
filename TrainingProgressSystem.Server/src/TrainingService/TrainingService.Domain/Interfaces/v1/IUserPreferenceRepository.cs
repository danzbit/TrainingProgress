using Shared.Kernal.Results;
using TrainingService.Domain.Entities;

namespace TrainingService.Domain.Interfaces.v1;

public interface IUserPreferenceRepository
{
    Task<ResultOfT<UserPreference?>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Result> UpsertAsync(UserPreference preference, CancellationToken ct = default);
}
