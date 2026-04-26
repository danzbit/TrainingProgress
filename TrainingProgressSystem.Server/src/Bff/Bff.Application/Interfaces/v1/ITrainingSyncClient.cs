using Bff.Application.Dtos.v1.Commands;
using Shared.Kernal.Results;
using Shared.Saga.Models;

namespace Bff.Application.Interfaces.v1;

public interface ITrainingSyncClient
{
    Task<ResultOfT<Guid>> CreateWorkoutAsync(CreateWorkoutCommand command, SagaCallContext context);

    Task<ResultOfT<Guid>> SaveGoalAsync(SaveGoalCommand command, SagaCallContext context);

    Task<Result> DeleteWorkoutAsync(Guid workoutId, SagaCallContext context);

    Task<Result> DeleteGoalAsync(Guid goalId, SagaCallContext context);

    Task<Result> UpdateGoalsForWorkoutAsync(Guid userId, Guid workoutId, SagaCallContext context);

    Task<Result> RecalculateProgressForGoalAsync(Guid userId, Guid goalId, SagaCallContext context);
}
