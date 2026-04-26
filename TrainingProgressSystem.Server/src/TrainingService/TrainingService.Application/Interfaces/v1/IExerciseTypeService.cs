using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Responses;

namespace TrainingService.Application.Interfaces.v1;

public interface IExerciseTypeService
{
    Task<ResultOfT<IReadOnlyList<ExerciseTypeResponse>>> GetAllExerciseTypesAsync();
}
