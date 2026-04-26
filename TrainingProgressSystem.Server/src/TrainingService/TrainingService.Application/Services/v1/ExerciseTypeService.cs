using Microsoft.Extensions.Logging;
using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;
using TrainingService.Domain.Interfaces.v1;

namespace TrainingService.Application.Services.v1;

public class ExerciseTypeService(
    IExerciseTypeRepository exerciseTypeRepository,
    ILogger<ExerciseTypeService> logger) : IExerciseTypeService
{
    public async Task<ResultOfT<IReadOnlyList<ExerciseTypeResponse>>> GetAllExerciseTypesAsync()
    {
        logger.LogInformation("Getting all exercise types");

        var result = await exerciseTypeRepository.GetAllAsync();

        if (result.IsFailure)
        {
            return ResultOfT<IReadOnlyList<ExerciseTypeResponse>>.Failure(result.Error);
        }

        var responses = result.Value
            .Select(et => new ExerciseTypeResponse(et.Id, et.Name, et.Category))
            .ToList();

        return ResultOfT<IReadOnlyList<ExerciseTypeResponse>>.Success(responses);
    }
}
