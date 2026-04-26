using Shared.Kernal.Results;
using TrainingService.Domain.Entities;

namespace TrainingService.Domain.Interfaces.v1;

public interface IWorkoutTypeRepository
{
    Task<ResultOfT<IReadOnlyList<WorkoutType>>> GetAllAsync(CancellationToken ct = default);
}
