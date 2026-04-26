using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;

namespace TrainingService.Application.Interfaces.v1;

public interface IUserPreferenceService
{
    Task<ResultOfT<UserPreferenceResponse>> GetPreferenceAsync(CancellationToken ct = default);
    Task<Result> UpdatePreferenceAsync(UpdateUserPreferenceRequest request, CancellationToken ct = default);
}
