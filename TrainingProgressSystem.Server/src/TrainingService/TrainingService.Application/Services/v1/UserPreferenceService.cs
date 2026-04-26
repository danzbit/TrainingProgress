using Shared.Abstractions.Auth;
using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Interfaces.v1;

namespace TrainingService.Application.Services.v1;

public class UserPreferenceService(
    IUserPreferenceRepository repository,
    ICurrentUser currentUser) : IUserPreferenceService
{
    private static readonly HashSet<string> AllowedViewModes = ["list", "calendar"];

    public async Task<ResultOfT<UserPreferenceResponse>> GetPreferenceAsync(CancellationToken ct = default)
    {
        var userIdResult = currentUser.GetCurrentUserId();
        if (userIdResult.IsFailure)
            return ResultOfT<UserPreferenceResponse>.Failure(userIdResult.Error);

        var result = await repository.GetByUserIdAsync(userIdResult.Value, ct);

        if (result.IsFailure)
            return ResultOfT<UserPreferenceResponse>.Failure(result.Error);

        var viewMode = result.Value?.HistoryViewMode ?? "list";
        return ResultOfT<UserPreferenceResponse>.Success(new UserPreferenceResponse(viewMode));
    }

    public async Task<Result> UpdatePreferenceAsync(UpdateUserPreferenceRequest request, CancellationToken ct = default)
    {
        var userIdResult = currentUser.GetCurrentUserId();
        if (userIdResult.IsFailure)
            return Result.Failure(userIdResult.Error);

        if (!AllowedViewModes.Contains(request.HistoryViewMode))
            return Result.Failure(new Shared.Kernal.Errors.Error(
                Shared.Kernal.Errors.ErrorCode.ValidationFailed,
                $"Invalid view mode '{request.HistoryViewMode}'. Allowed: list, calendar."));

        var preference = new UserPreference
        {
            UserId = userIdResult.Value,
            HistoryViewMode = request.HistoryViewMode,
        };

        return await repository.UpsertAsync(preference, ct);
    }
}
