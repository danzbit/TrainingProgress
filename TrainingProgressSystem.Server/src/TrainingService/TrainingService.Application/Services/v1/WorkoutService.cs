using AutoMapper;
using Microsoft.Extensions.Logging;
using Shared.Kernal.Results;
using TrainingService.Application.Builders.v1;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Interfaces.v1;

namespace TrainingService.Application.Services.v1;

public class WorkoutService(
    IWorkoutRepository workoutRepository,
    IMapper mapper,
    ILogger<WorkoutService> logger) : IWorkoutService
{
    public async Task<ResultOfT<IReadOnlyList<WorkoutsListItemResponse>>> GetAllWorkoutsAsync()
    {
        logger.LogInformation("Getting all workouts");

        var result = await workoutRepository.GetAllAsync();

        if (result.IsFailure)
        {
            return ResultOfT<IReadOnlyList<WorkoutsListItemResponse>>.Failure(result.Error);
        }

        var listItems = result.Value
            .Select(workout => new WorkoutsListItemResponse(
                workout.WorkoutType?.Name ?? string.Empty,
                workout.DurationMin,
                workout.Exercises?.Count ?? 0,
                workout.Date))
            .ToList();

        return ResultOfT<IReadOnlyList<WorkoutsListItemResponse>>.Success(listItems);
    }

    public async Task<ResultOfT<WorkoutsResponse>> GetWorkoutAsync(Guid workoutId)
    {
        logger.LogInformation("Getting workout with ID: {WorkoutId}", workoutId);

        var result = await workoutRepository.GetByIdAsync(workoutId);

        return !result.IsFailure
            ? ResultOfT<WorkoutsResponse>.Success(mapper.Map<WorkoutsResponse>(result.Value))
            : ResultOfT<WorkoutsResponse>.Failure(result.Error);
    }

    public async Task<ResultOfT<CreateWorkoutResponse>> CreateWorkoutAsync(CreateWorkoutRequest request)
    {
        logger.LogInformation("Creating a new workout");

        var builder = new WorkoutBuilder(request.UserId, request.WorkoutTypeId, request.Date);

        if (request.DurationMin.HasValue)
            builder.WithDuration(request.DurationMin.Value);

        if (!string.IsNullOrEmpty(request.Notes))
            builder.WithNotes(request.Notes);

        if (request.Exercises != null)
            foreach (var ex in request.Exercises)
                builder.ApplyExercises([ex]);

        var workout = builder.Build();        

        var createResult = await workoutRepository.AddAsync(workout);

        return createResult.IsFailure
            ? ResultOfT<CreateWorkoutResponse>.Failure(createResult.Error)
            : ResultOfT<CreateWorkoutResponse>.Success(new CreateWorkoutResponse(workout.Id));
    }

    public async Task<Result> UpdateWorkoutAsync(UpdateWorkoutRequest request)
    {
        logger.LogInformation("Updating workout with ID: {WorkoutId}", request.WorkoutId);

        var builder = new WorkoutBuilder(request, mapper)
            .WithDuration(request.DurationMin)
            .WithNotes(request.Notes)
            .ApplyExercises(request.Exercises);

        var updatedWorkout = builder.Build();

        return await workoutRepository.UpdateAsync(updatedWorkout);
    }

    public async Task<Result> DeleteWorkoutAsync(Guid workoutId)
    {
        logger.LogInformation("Deleting workout with ID: {WorkoutId}", workoutId);

        return await workoutRepository.DeleteAsync(workoutId);
    }
}