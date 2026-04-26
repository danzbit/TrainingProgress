using Bff.Application.Interfaces.v1;
using Bff.Infrastructure.Helpers;
using Shared.Grpc.Contracts;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using Shared.Saga.Models;

namespace Bff.Infrastructure.Clients;

internal sealed class NotificationSyncGrpcClient(NotificationSyncGrpc.NotificationSyncGrpcClient client)
    : INotificationSyncClient
{
    public async Task<Result> ResetRemindersForWorkoutAsync(Guid userId, Guid workoutId, SagaCallContext context)
    {
        var response = await client.ResetRemindersForWorkoutAsync(
            new ResetRemindersForWorkoutGrpcRequest
            {
                UserId = userId.ToString(),
                WorkoutId = workoutId.ToString()
            },
            GrpcCallContextHelper.BuildHeaders(context),
            GrpcCallContextHelper.BuildDeadline(context),
            context.CancellationToken);

        return response.IsSuccess
            ? Result.Success()
            : Result.Failure(new Error(ErrorCode.DownstreamServiceUnavailable, response.Error));
    }

    public async Task<Result> ScheduleRemindersForGoalAsync(Guid userId, Guid goalId, SagaCallContext context)
    {
        var response = await client.ScheduleRemindersForGoalAsync(
            new ScheduleRemindersForGoalGrpcRequest
            {
                UserId = userId.ToString(),
                GoalId = goalId.ToString()
            },
            GrpcCallContextHelper.BuildHeaders(context),
            GrpcCallContextHelper.BuildDeadline(context),
            context.CancellationToken);

        return response.IsSuccess
            ? Result.Success()
            : Result.Failure(new Error(ErrorCode.DownstreamServiceUnavailable, response.Error));
    }
}
