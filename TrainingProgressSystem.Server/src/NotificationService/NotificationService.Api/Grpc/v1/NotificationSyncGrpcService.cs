using Grpc.Core;
using NotificationService.Application.Interfaces.v1;
using Shared.Grpc.Contracts;

namespace NotificationService.Api.Grpc.v1;

public sealed class NotificationSyncGrpcService(
    INotificationSyncService syncService,
    ILogger<NotificationSyncGrpcService> logger)
    : Shared.Grpc.Contracts.NotificationSyncGrpc.NotificationSyncGrpcBase
{
    public override async Task<ResetRemindersForWorkoutGrpcResponse> ResetRemindersForWorkout(
        ResetRemindersForWorkoutGrpcRequest request,
        ServerCallContext context)
    {
        logger.LogInformation("Received ResetRemindersForWorkout gRPC call. UserId: {UserId}, WorkoutId: {WorkoutId}",
            request.UserId,
            request.WorkoutId);

        if (!Guid.TryParse(request.UserId, out var userId))
        {
            return new ResetRemindersForWorkoutGrpcResponse { IsSuccess = false, Error = "Invalid UserId format." };
        }

        await syncService.ResetRemindersForWorkoutAsync(userId, context.CancellationToken);

        return new ResetRemindersForWorkoutGrpcResponse { IsSuccess = true };
    }

    public override async Task<ScheduleRemindersForGoalGrpcResponse> ScheduleRemindersForGoal(
        ScheduleRemindersForGoalGrpcRequest request,
        ServerCallContext context)
    {
        logger.LogInformation(
            "Received ScheduleRemindersForGoal gRPC call. UserId: {UserId}, GoalId: {GoalId}",
            request.UserId,
            request.GoalId);

        if (!Guid.TryParse(request.UserId, out var userId) || !Guid.TryParse(request.GoalId, out var goalId))
        {
            return new ScheduleRemindersForGoalGrpcResponse { IsSuccess = false, Error = "Invalid UserId or GoalId format." };
        }

        await syncService.ScheduleRemindersForGoalAsync(userId, goalId, context.CancellationToken);

        return new ScheduleRemindersForGoalGrpcResponse { IsSuccess = true };
    }
}
