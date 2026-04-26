using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;

namespace TrainingService.Application.Interfaces.v1;

public interface IWorkoutService
{
    Task<ResultOfT<IReadOnlyList<WorkoutsListItemResponse>>> GetAllWorkoutsAsync();

    Task<ResultOfT<WorkoutsResponse>> GetWorkoutAsync(Guid workoutId);

    Task<ResultOfT<CreateWorkoutResponse>> CreateWorkoutAsync(CreateWorkoutRequest createWorkoutRequest);

    Task<Result> UpdateWorkoutAsync(UpdateWorkoutRequest request);

    Task<Result> DeleteWorkoutAsync(Guid workoutId);
}