using AnalyticsService.Application.Dtos.v1.Responses;
using Shared.Kernal.Results;

namespace AnalyticsService.Application.Interfaces.v1;

public interface IAnalyticsSnapshotService
{
    Task<ResultOfT<AnalyticsSnapshotData>> GetSnapshotAsync(Guid userId, CancellationToken ct = default);

    Task<ResultOfT<AnalyticsSnapshotData>> RefreshSnapshotAsync(Guid userId, CancellationToken ct = default);
}
