using AnalyticsService.Domain.Entities;
using Shared.Kernal.Results;

namespace AnalyticsService.Domain.Interfaces.v1;

public interface IAnalyticsSnapshotRepository
{
    Task<ResultOfT<AnalyticsSnapshot?>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<Result> UpsertAsync(AnalyticsSnapshot snapshot, CancellationToken ct = default);
}
