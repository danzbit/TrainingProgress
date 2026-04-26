using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;

namespace TrainingService.Api.Integration.Infrastructure;

public sealed class StubUserPreferenceService : IUserPreferenceService
{
    public Func<CancellationToken, Task<ResultOfT<UserPreferenceResponse>>> GetPreferenceHandler { get; set; } = default!;
    public Func<UpdateUserPreferenceRequest, CancellationToken, Task<Result>> UpdatePreferenceHandler { get; set; } = default!;

    public StubUserPreferenceService() => Reset();

    public Task<ResultOfT<UserPreferenceResponse>> GetPreferenceAsync(CancellationToken ct = default)
        => GetPreferenceHandler(ct);

    public Task<Result> UpdatePreferenceAsync(UpdateUserPreferenceRequest request, CancellationToken ct = default)
        => UpdatePreferenceHandler(request, ct);

    public void Reset()
    {
        GetPreferenceHandler = _ => Task.FromResult(ResultOfT<UserPreferenceResponse>.Success(
            new UserPreferenceResponse("List")));

        UpdatePreferenceHandler = (_, _) => Task.FromResult(Result.Success());
    }
}
