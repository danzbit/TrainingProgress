using Grpc.Core;
using AnalyticsService.Application.Interfaces.v1;
using Shared.Grpc.Contracts;

namespace AnalyticsService.Api.Grpc.v1;

public sealed class AnalyticsSyncGrpcService(
    IAnalyticsSyncService syncService,
    ILogger<AnalyticsSyncGrpcService> logger)
    : Shared.Grpc.Contracts.AnalyticsSyncGrpc.AnalyticsSyncGrpcBase
{
    public override async Task<RecalculateForWorkoutGrpcResponse> RecalculateForWorkout(
        RecalculateForWorkoutGrpcRequest request,
        ServerCallContext context)
    {
        logger.LogInformation("Received RecalculateForWorkout gRPC call. UserId: {UserId}, WorkoutId: {WorkoutId}",
            request.UserId,
            request.WorkoutId);

        if (!Guid.TryParse(request.UserId, out var userId) || !Guid.TryParse(request.WorkoutId, out var workoutId))
        {
            return new RecalculateForWorkoutGrpcResponse
            {
                IsSuccess = false,
                Error = "Invalid UserId or WorkoutId format."
            };
        }

        var result = await syncService.RecalculateForWorkoutAsync(userId, workoutId, context.CancellationToken);
        if (result.IsFailure)
        {
            logger.LogWarning(
                "RecalculateForWorkout failed for UserId: {UserId}, WorkoutId: {WorkoutId}. Error: {Error}",
                userId,
                workoutId,
                result.Error.Description);

            return new RecalculateForWorkoutGrpcResponse
            {
                IsSuccess = false,
                Error = result.Error.Description ?? "Failed to recalculate analytics for workout."
            };
        }

        return new RecalculateForWorkoutGrpcResponse { IsSuccess = true };
    }
}
