using Shared.Kernal.Results;
using TrainingService.Domain.Entities;

namespace TrainingService.Domain.Interfaces.v1;

public interface IWorkoutRepository
{
    Task<ResultOfT<IReadOnlyList<Workout>>> GetAllAsync(CancellationToken ct = default);

    Task<ResultOfT<Workout?>> GetByIdAsync(Guid workoutId, CancellationToken ct = default);

    Task<Result> AddAsync(Workout workout, CancellationToken ct = default);

    Task<Result> UpdateAsync(Workout workout, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid workoutId, CancellationToken ct = default);
}