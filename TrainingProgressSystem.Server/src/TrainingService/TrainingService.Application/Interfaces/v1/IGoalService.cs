using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;

namespace TrainingService.Application.Interfaces.v1;

public interface IGoalService
{
    Task<ResultOfT<IReadOnlyList<GoalsListItemResponse>>> GetAllGoalsAsync();

    Task<ResultOfT<GoalsResponse>> GetGoalAsync(Guid goalId);

    Task<ResultOfT<Guid>> CreateGoalAsync(CreateGoalRequest request, CancellationToken ct = default);

    Task<Result> UpdateGoalAsync(UpdateGoalRequest request, CancellationToken ct = default);

    Task<Result> DeleteGoalAsync(Guid goalId, CancellationToken ct = default);

    Task<Result> UpdateGoalsForWorkoutAsync(Guid userId, Guid workoutId, CancellationToken ct = default);

    Task<Result> RecalculateProgressForGoalAsync(Guid userId, Guid goalId, CancellationToken ct = default);
}
