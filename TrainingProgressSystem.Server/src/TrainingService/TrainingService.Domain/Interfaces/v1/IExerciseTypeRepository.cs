using Shared.Kernal.Results;
using TrainingService.Domain.Entities;

namespace TrainingService.Domain.Interfaces.v1;

public interface IExerciseTypeRepository
{
    Task<ResultOfT<IReadOnlyList<ExerciseType>>> GetAllAsync(CancellationToken ct = default);
}
