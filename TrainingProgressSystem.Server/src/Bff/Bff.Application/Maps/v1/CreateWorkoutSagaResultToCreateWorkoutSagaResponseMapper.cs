using Bff.Application.Dtos.v1.Responses;
using Shared.Kernal.Results;

namespace Bff.Application.Maps.v1;

public static class CreateWorkoutSagaResultToCreateWorkoutSagaResponseMapper
{
    public static CreateWorkoutSagaResponse ToCreateWorkoutSagaResponse(this ResultOfT<CreateWorkoutSagaResult> result)
        => new(result.Value?.WorkoutId);
}