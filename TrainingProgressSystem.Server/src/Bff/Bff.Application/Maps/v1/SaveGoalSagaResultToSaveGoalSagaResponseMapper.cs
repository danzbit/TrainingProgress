using Bff.Application.Dtos.v1.Responses;
using Shared.Kernal.Results;

namespace Bff.Application.Maps.v1;

public static class SaveGoalSagaResultToSaveGoalSagaResponseMapper
{
    public static SaveGoalSagaResponse ToSaveGoalSagaResponse(this ResultOfT<SaveGoalSagaResult> result)
        => new(result.Value?.GoalId);
}
