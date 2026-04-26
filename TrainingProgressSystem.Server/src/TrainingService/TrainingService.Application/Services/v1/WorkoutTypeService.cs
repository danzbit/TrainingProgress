using Microsoft.Extensions.Logging;
using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;
using TrainingService.Domain.Interfaces.v1;

namespace TrainingService.Application.Services.v1;

public class WorkoutTypeService(
    IWorkoutTypeRepository workoutTypeRepository,
    ILogger<WorkoutTypeService> logger) : IWorkoutTypeService
{
    public async Task<ResultOfT<IReadOnlyList<WorkoutTypeResponse>>> GetAllWorkoutTypesAsync()
    {
        logger.LogInformation("Getting all workout types");

        var result = await workoutTypeRepository.GetAllAsync();

        if (result.IsFailure)
        {
            return ResultOfT<IReadOnlyList<WorkoutTypeResponse>>.Failure(result.Error);
        }

        var responses = result.Value
            .Select(wt => new WorkoutTypeResponse(wt.Id, wt.Name, wt.Description))
            .ToList();

        return ResultOfT<IReadOnlyList<WorkoutTypeResponse>>.Success(responses);
    }
}
