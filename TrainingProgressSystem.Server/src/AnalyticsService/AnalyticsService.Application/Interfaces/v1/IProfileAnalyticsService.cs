using AnalyticsService.Application.Dtos.v1.Responses;
using Shared.Kernal.Results;

namespace AnalyticsService.Application.Interfaces.v1;

public interface IProfileAnalyticsService
{
    Task<ResultOfT<ProfileAnalyticsResponse>> GetProfileAnalyticsAsync(CancellationToken ct = default);
}