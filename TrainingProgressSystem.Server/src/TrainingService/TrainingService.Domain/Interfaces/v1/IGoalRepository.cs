using Shared.Kernal.Results;
using TrainingService.Domain.Entities;

namespace TrainingService.Domain.Interfaces.v1;

public interface IGoalRepository
{
    Task<ResultOfT<IReadOnlyList<Goal>>> GetAllAsync(CancellationToken ct = default);

    Task<ResultOfT<Goal?>> GetByIdAsync(Guid goalId, CancellationToken ct = default);

    Task<Result> AddAsync(Goal goal, CancellationToken ct = default);

    Task<Result> UpdateAsync(Goal goal, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid goalId, CancellationToken ct = default);

    Task<Result> UpdateGoalsForWorkoutAsync(Guid userId, Guid workoutId, CancellationToken ct = default);

    Task<ResultOfT<int>> RecalculateGoalsProgressAsync(Guid userId, Guid? goalId = null, CancellationToken ct = default);
}
