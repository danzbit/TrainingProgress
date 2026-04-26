using Grpc.Core;
using Shared.Grpc.Contracts;
using TrainingService.Api.Maps.v1;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Interfaces.v1;
using TrainingService.Domain.Enums;

namespace TrainingService.Api.Grpc.v1;

public sealed class TrainingSyncGrpcService(
    IWorkoutService workoutService,
    IGoalService goalService,
    ILogger<TrainingSyncGrpcService> logger) : TrainingSyncGrpc.TrainingSyncGrpcBase
{
    public override async Task<CreateWorkoutGrpcResponse> CreateWorkout(CreateWorkoutGrpcRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId) || !Guid.TryParse(request.WorkoutTypeId, out var workoutTypeId))
        {
            return new CreateWorkoutGrpcResponse { IsSuccess = false, Error = "Invalid userId or workoutTypeId." };
        }

        var workoutRequest = new CreateWorkoutRequest(
            userId,
            request.Date.ToDateTime(),
            workoutTypeId,
            request.HasDurationMin ? request.DurationMin : null,
            string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes,
            request.Exercises.ToExerciseRequests());

        var result = await workoutService.CreateWorkoutAsync(workoutRequest);
        if (result.IsFailure)
        {
            logger.LogWarning("gRPC CreateWorkout failed: {Error}", result.Error.Description);
            return new CreateWorkoutGrpcResponse
            {
                IsSuccess = false,
                Error = result.Error.Description ?? "Failed to create workout."
            };
        }

        return new CreateWorkoutGrpcResponse
        {
            IsSuccess = true,
            WorkoutId = result.Value.WorkoutId.ToString()
        };
    }

    public override async Task<SaveGoalGrpcResponse> SaveGoal(SaveGoalGrpcRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
        {
            return new SaveGoalGrpcResponse { IsSuccess = false, Error = "Invalid userId." };
        }

        if (!Enum.IsDefined(typeof(GoalMetricType), request.MetricType))
        {
            return new SaveGoalGrpcResponse { IsSuccess = false, Error = "Invalid metricType." };
        }

        if (!Enum.IsDefined(typeof(GoalPeriodType), request.PeriodType))
        {
            return new SaveGoalGrpcResponse { IsSuccess = false, Error = "Invalid periodType." };
        }

        logger.LogInformation("Received SaveGoal gRPC call. UserId: {UserId}, Name: {Name}, MetricType: {MetricType}, PeriodType: {PeriodType}, TargetValue: {TargetValue}",
            userId,
            request.Name,
            request.MetricType,
            request.PeriodType,
            request.TargetValue);

        var createGoalRequest = new CreateGoalRequest(
            userId,
            request.Name,
            request.Description,
            (GoalMetricType)request.MetricType,
            (GoalPeriodType)request.PeriodType,
            request.TargetValue,
            request.StartDate.ToDateTime(),
            request.HasEndDate ? request.EndDate.ToDateTime() : null);

        var result = await goalService.CreateGoalAsync(createGoalRequest, context.CancellationToken);
        if (result.IsFailure)
        {
            logger.LogWarning("gRPC SaveGoal failed: {Error}", result.Error.Description);
            return new SaveGoalGrpcResponse
            {
                IsSuccess = false,
                Error = result.Error.Description ?? "Failed to save goal."
            };
        }

        return new SaveGoalGrpcResponse
        {
            IsSuccess = true,
            GoalId = result.Value.ToString()
        };
    }

    public override async Task<DeleteWorkoutGrpcResponse> DeleteWorkout(DeleteWorkoutGrpcRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.WorkoutId, out var workoutId))
        {
            return new DeleteWorkoutGrpcResponse { IsSuccess = false, Error = "Invalid workoutId." };
        }

        var result = await workoutService.DeleteWorkoutAsync(workoutId);
        return new DeleteWorkoutGrpcResponse
        {
            IsSuccess = !result.IsFailure,
            Error = result.IsFailure ? result.Error.Description ?? "Failed to delete workout." : string.Empty
        };
    }

    public override async Task<DeleteGoalGrpcResponse> DeleteGoal(DeleteGoalGrpcRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.GoalId, out var goalId))
        {
            return new DeleteGoalGrpcResponse { IsSuccess = false, Error = "Invalid goalId." };
        }

        logger.LogInformation("Received DeleteGoal gRPC call. GoalId: {GoalId}", goalId);

        var result = await goalService.DeleteGoalAsync(goalId, context.CancellationToken);
        if (result.IsFailure)
        {
            logger.LogWarning("DeleteGoal failed. GoalId: {GoalId}, Error: {Error}", goalId, result.Error.Description);
            return new DeleteGoalGrpcResponse
            {
                IsSuccess = false,
                Error = result.Error.Description ?? "Failed to delete goal."
            };
        }

        return new DeleteGoalGrpcResponse { IsSuccess = true };
    }

    public override async Task<UpdateGoalsForWorkoutGrpcResponse> UpdateGoalsForWorkout(
        UpdateGoalsForWorkoutGrpcRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId) || !Guid.TryParse(request.WorkoutId, out var workoutId))
        {
            return new UpdateGoalsForWorkoutGrpcResponse
            {
                IsSuccess = false,
                Error = "Invalid userId or workoutId."
            };
        }

        logger.LogInformation("Received UpdateGoalsForWorkout gRPC call. UserId: {UserId}, WorkoutId: {WorkoutId}",
            request.UserId, request.WorkoutId);


        var result = await goalService.UpdateGoalsForWorkoutAsync(userId, workoutId, context.CancellationToken);
        if (result.IsFailure)
        {
            logger.LogWarning("UpdateGoalsForWorkout failed. UserId: {UserId}, WorkoutId: {WorkoutId}, Error: {Error}",
                userId,
                workoutId,
                result.Error.Description);

            return new UpdateGoalsForWorkoutGrpcResponse
            {
                IsSuccess = false,
                Error = result.Error.Description ?? "Failed to update goals for workout."
            };
        }

        return new UpdateGoalsForWorkoutGrpcResponse { IsSuccess = true };
    }

    public override async Task<RecalculateProgressForGoalGrpcResponse> RecalculateProgressForGoal(
        RecalculateProgressForGoalGrpcRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId) || !Guid.TryParse(request.GoalId, out var goalId))
        {
            return new RecalculateProgressForGoalGrpcResponse
            {
                IsSuccess = false,
                Error = "Invalid userId or goalId."
            };
        }

        logger.LogInformation("Received RecalculateProgressForGoal gRPC call. UserId: {UserId}, GoalId: {GoalId}",
            userId, goalId);

        var result = await goalService.RecalculateProgressForGoalAsync(userId, goalId, context.CancellationToken);
        if (result.IsFailure)
        {
            logger.LogWarning("RecalculateProgressForGoal failed. UserId: {UserId}, GoalId: {GoalId}, Error: {Error}",
                userId,
                goalId,
                result.Error.Description);

            return new RecalculateProgressForGoalGrpcResponse
            {
                IsSuccess = false,
                Error = result.Error.Description ?? "Failed to recalculate progress for goal."
            };
        }

        return new RecalculateProgressForGoalGrpcResponse { IsSuccess = true };
    }
}
