using Shared.Kernal.Results;
using Shared.Saga.Models;

namespace Bff.Application.Interfaces.v1;

public interface IAnalyticsSyncClient
{
    Task<Result> RecalculateForWorkoutAsync(Guid userId, Guid workoutId, SagaCallContext context);
}
