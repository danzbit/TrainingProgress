using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Responses;

namespace TrainingService.Application.Interfaces.v1;

public interface IWorkoutTypeService
{
    Task<ResultOfT<IReadOnlyList<WorkoutTypeResponse>>> GetAllWorkoutTypesAsync();
}
