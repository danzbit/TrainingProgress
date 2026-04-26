using Google.Protobuf.WellKnownTypes;
using Bff.Application.Dtos.v1.Commands;
using Bff.Application.Interfaces.v1;
using Bff.Infrastructure.Helpers;
using Shared.Grpc.Contracts;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using Shared.Saga.Models;

namespace Bff.Infrastructure.Clients;

internal sealed class TrainingSyncGrpcClient(TrainingSyncGrpc.TrainingSyncGrpcClient client)
    : ITrainingSyncClient
{
    public async Task<ResultOfT<Guid>> CreateWorkoutAsync(CreateWorkoutCommand command, SagaCallContext context)
    {
        var request = new CreateWorkoutGrpcRequest
        {
            UserId = command.UserId.ToString(),
            WorkoutTypeId = command.WorkoutTypeId.ToString(),
            Date = Timestamp.FromDateTime(DateTime.SpecifyKind(command.Date, DateTimeKind.Utc)),
            Notes = command.Notes ?? string.Empty,
            HasDurationMin = command.DurationMin.HasValue,
            DurationMin = command.DurationMin ?? 0
        };

        if (command.Exercises != null)
        {
            request.Exercises.AddRange(command.Exercises.Select(ex => new CreateWorkoutExerciseGrpcRequest
            {
                HasExerciseId = ex.ExerciseId.HasValue,
                ExerciseId = ex.ExerciseId?.ToString() ?? string.Empty,
                ExerciseTypeId = ex.ExerciseTypeId.ToString(),
                Sets = ex.Sets,
                Reps = ex.Reps,
                HasDurationSec = ex.DurationSec.HasValue,
                DurationSec = ex.DurationSec ?? 0,
                HasWeightKg = ex.WeightKg.HasValue,
                WeightKg = ex.WeightKg.HasValue ? Convert.ToDouble(ex.WeightKg.Value) : 0
            }));
        }

        var response = await client.CreateWorkoutAsync(
            request,
            GrpcCallContextHelper.BuildHeaders(context),
            GrpcCallContextHelper.BuildDeadline(context),
            context.CancellationToken);

        if (!response.IsSuccess)
        {
            return ResultOfT<Guid>.Failure(new Error(ErrorCode.DownstreamServiceUnavailable, response.Error));
        }

        return Guid.TryParse(response.WorkoutId, out var workoutId)
            ? ResultOfT<Guid>.Success(workoutId)
            : ResultOfT<Guid>.Failure(new Error(ErrorCode.DeserializationFailed, "WorkoutId is invalid in gRPC response."));
    }

    public async Task<ResultOfT<Guid>> SaveGoalAsync(SaveGoalCommand command, SagaCallContext context)
    {
        var response = await client.SaveGoalAsync(
            new SaveGoalGrpcRequest
            {
                UserId = command.UserId.ToString(),
                Name = command.Name,
                Description = command.Description,
                MetricType = command.MetricType,
                PeriodType = command.PeriodType,
                TargetValue = command.TargetValue,
                StartDate = Timestamp.FromDateTime(DateTime.SpecifyKind(command.StartDate, DateTimeKind.Utc)),
                HasEndDate = command.EndDate.HasValue,
                EndDate = command.EndDate.HasValue
                    ? Timestamp.FromDateTime(DateTime.SpecifyKind(command.EndDate.Value, DateTimeKind.Utc))
                    : new Timestamp()
            },
            GrpcCallContextHelper.BuildHeaders(context),
            GrpcCallContextHelper.BuildDeadline(context),
            context.CancellationToken);

        if (!response.IsSuccess)
        {
            return ResultOfT<Guid>.Failure(new Error(ErrorCode.DownstreamServiceUnavailable, response.Error));
        }

        return Guid.TryParse(response.GoalId, out var goalId)
            ? ResultOfT<Guid>.Success(goalId)
            : ResultOfT<Guid>.Failure(new Error(ErrorCode.DeserializationFailed, "GoalId is invalid in gRPC response."));
    }

    public async Task<Result> DeleteWorkoutAsync(Guid workoutId, SagaCallContext context)
    {
        var response = await client.DeleteWorkoutAsync(
            new DeleteWorkoutGrpcRequest { WorkoutId = workoutId.ToString() },
            GrpcCallContextHelper.BuildHeaders(context),
            GrpcCallContextHelper.BuildDeadline(context),
            context.CancellationToken);

        return response.IsSuccess
            ? Result.Success()
            : Result.Failure(new Error(ErrorCode.DownstreamServiceUnavailable, response.Error));
    }

    public async Task<Result> DeleteGoalAsync(Guid goalId, SagaCallContext context)
    {
        var response = await client.DeleteGoalAsync(
            new DeleteGoalGrpcRequest { GoalId = goalId.ToString() },
            GrpcCallContextHelper.BuildHeaders(context),
            GrpcCallContextHelper.BuildDeadline(context),
            context.CancellationToken);

        return response.IsSuccess
            ? Result.Success()
            : Result.Failure(new Error(ErrorCode.DownstreamServiceUnavailable, response.Error));
    }

    public async Task<Result> UpdateGoalsForWorkoutAsync(Guid userId, Guid workoutId, SagaCallContext context)
    {
        var response = await client.UpdateGoalsForWorkoutAsync(
            new UpdateGoalsForWorkoutGrpcRequest
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

    public async Task<Result> RecalculateProgressForGoalAsync(Guid userId, Guid goalId, SagaCallContext context)
    {
        var response = await client.RecalculateProgressForGoalAsync(
            new RecalculateProgressForGoalGrpcRequest
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
