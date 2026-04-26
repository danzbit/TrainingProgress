using Shared.Kernal.Results;
using Shared.Saga.Models;

namespace Bff.Application.Interfaces.v1;

public interface INotificationSyncClient
{
    Task<Result> ResetRemindersForWorkoutAsync(Guid userId, Guid workoutId, SagaCallContext context);

    Task<Result> ScheduleRemindersForGoalAsync(Guid userId, Guid goalId, SagaCallContext context);
}
