using AutoMapper;
using Microsoft.Extensions.Logging;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Enums;
using TrainingService.Domain.Interfaces.v1;

namespace TrainingService.Application.Services.v1;

public class GoalService(
    IGoalRepository goalRepository,
    IMapper mapper,
    ILogger<GoalService> logger) : IGoalService
{
    public async Task<ResultOfT<IReadOnlyList<GoalsListItemResponse>>> GetAllGoalsAsync()
    {
        logger.LogInformation("Getting all goals");

        var result = await goalRepository.GetAllAsync();

        if (result.IsFailure)
        {
            return ResultOfT<IReadOnlyList<GoalsListItemResponse>>.Failure(result.Error);
        }

        var listItems = result.Value
            .Select(goal =>
            {
                var currentValue = goal.Progress?.CurrentValue
                                   ?? (goal.Status == GoalStatus.Completed ? goal.TargetValue : 0);
                var progressPercentage = goal.Progress?.Percentage
                                         ?? (goal.Status == GoalStatus.Completed ? 100d : 0d);
                var isCompleted = goal.Progress?.IsCompleted
                                  ?? goal.Status == GoalStatus.Completed;
                var lastCalculatedAt = goal.Progress?.LastCalculatedAt;

                return new GoalsListItemResponse(
                    goal.Id,
                    goal.Name,
                    goal.Description,
                    goal.MetricType,
                    goal.PeriodType,
                    goal.Status,
                    goal.TargetValue,
                    goal.StartDate,
                    goal.EndDate,
                    new GoalProgressInfoResponse(
                        currentValue,
                        progressPercentage,
                        isCompleted,
                        lastCalculatedAt));
            })
            .ToList();

        return ResultOfT<IReadOnlyList<GoalsListItemResponse>>.Success(listItems);
    }

    public async Task<ResultOfT<GoalsResponse>> GetGoalAsync(Guid goalId)
    {
        logger.LogInformation("Getting goal with ID: {GoalId}", goalId);

        var result = await goalRepository.GetByIdAsync(goalId);
        if (result.IsFailure)
        {
            return ResultOfT<GoalsResponse>.Failure(result.Error);
        }

        if (result.Value is null)
        {
            return ResultOfT<GoalsResponse>.Failure(Error.EntityNotFound);
        }

        return ResultOfT<GoalsResponse>.Success(mapper.Map<GoalsResponse>(result.Value));
    }

    public async Task<ResultOfT<Guid>> CreateGoalAsync(CreateGoalRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Creating goal for user {UserId}", request.UserId);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ResultOfT<Guid>.Failure(new Error(ErrorCode.ValidationFailed, "Goal name is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return ResultOfT<Guid>.Failure(new Error(ErrorCode.ValidationFailed, "Goal description is required."));
        }

        if (request.TargetValue <= 0)
        {
            return ResultOfT<Guid>.Failure(new Error(ErrorCode.ValidationFailed, "TargetValue must be greater than 0."));
        }

        if (request.EndDate.HasValue && request.EndDate.Value.Date < request.StartDate.Date)
        {
            return ResultOfT<Guid>.Failure(new Error(ErrorCode.ValidationFailed, "EndDate must be greater than or equal to StartDate."));
        }

        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            MetricType = request.MetricType,
            PeriodType = request.PeriodType,
            TargetValue = request.TargetValue,
            Status = GoalStatus.Active,
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate?.Date
        };

        var result = await goalRepository.AddAsync(goal, ct);

        return result.IsFailure
            ? ResultOfT<Guid>.Failure(result.Error)
            : ResultOfT<Guid>.Success(goal.Id);
    }

    public async Task<Result> UpdateGoalAsync(UpdateGoalRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Updating goal with ID: {GoalId}", request.GoalId);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure(new Error(ErrorCode.ValidationFailed, "Goal name is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return Result.Failure(new Error(ErrorCode.ValidationFailed, "Goal description is required."));
        }

        if (request.TargetValue <= 0)
        {
            return Result.Failure(new Error(ErrorCode.ValidationFailed, "TargetValue must be greater than 0."));
        }

        if (request.EndDate.HasValue && request.EndDate.Value.Date < request.StartDate.Date)
        {
            return Result.Failure(new Error(ErrorCode.ValidationFailed, "EndDate must be greater than or equal to StartDate."));
        }

        var goalResult = await goalRepository.GetByIdAsync(request.GoalId, ct);
        if (goalResult.IsFailure)
        {
            return Result.Failure(goalResult.Error);
        }

        if (goalResult.Value is null)
        {
            return Result.Failure(Error.EntityNotFound);
        }

        var goal = goalResult.Value;

        goal.UserId = request.UserId;
        goal.Name = request.Name.Trim();
        goal.Description = request.Description.Trim();
        goal.MetricType = request.MetricType;
        goal.PeriodType = request.PeriodType;
        goal.TargetValue = request.TargetValue;
        goal.StartDate = request.StartDate.Date;
        goal.EndDate = request.EndDate?.Date;

        return await goalRepository.UpdateAsync(goal, ct);
    }

    public async Task<Result> DeleteGoalAsync(Guid goalId, CancellationToken ct = default)
    {
        logger.LogInformation("Deleting goal with ID: {GoalId}", goalId);

        return await goalRepository.DeleteAsync(goalId, ct);
    }

    public async Task<Result> UpdateGoalsForWorkoutAsync(Guid userId, Guid workoutId, CancellationToken ct = default)
    {
        logger.LogInformation("Updating goals for workout. UserId: {UserId}, WorkoutId: {WorkoutId}", userId, workoutId);

        return await goalRepository.UpdateGoalsForWorkoutAsync(userId, workoutId, ct);
    }

    public async Task<Result> RecalculateProgressForGoalAsync(Guid userId, Guid goalId, CancellationToken ct = default)
    {
        logger.LogInformation("Recalculating progress for all active goals of user. UserId: {UserId}", userId);

        // Call repository to recalculate progress for all active goals of the user
        var recalcResult = await goalRepository.RecalculateGoalsProgressAsync(userId, goalId, ct);
        if (recalcResult.IsFailure)
        {
            return Result.Failure(recalcResult.Error);
        }

        logger.LogInformation("Successfully recalculated progress for {Count} active goals of user {UserId}", recalcResult.Value, userId);
        return Result.Success();
    }
}
