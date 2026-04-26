using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;

namespace TrainingService.Api.Integration.Infrastructure;

public sealed class StubWorkoutService : IWorkoutService
{
    public Func<Task<ResultOfT<IReadOnlyList<WorkoutsListItemResponse>>>> GetAllHandler { get; set; } = default!;
    public Func<Guid, Task<ResultOfT<WorkoutsResponse>>> GetByIdHandler { get; set; } = default!;
    public Func<CreateWorkoutRequest, Task<ResultOfT<CreateWorkoutResponse>>> CreateHandler { get; set; } = default!;
    public Func<UpdateWorkoutRequest, Task<Result>> UpdateHandler { get; set; } = default!;
    public Func<Guid, Task<Result>> DeleteHandler { get; set; } = default!;

    public StubWorkoutService() => Reset();

    public Task<ResultOfT<IReadOnlyList<WorkoutsListItemResponse>>> GetAllWorkoutsAsync() => GetAllHandler();
    public Task<ResultOfT<WorkoutsResponse>> GetWorkoutAsync(Guid workoutId) => GetByIdHandler(workoutId);
    public Task<ResultOfT<CreateWorkoutResponse>> CreateWorkoutAsync(CreateWorkoutRequest request) => CreateHandler(request);
    public Task<Result> UpdateWorkoutAsync(UpdateWorkoutRequest request) => UpdateHandler(request);
    public Task<Result> DeleteWorkoutAsync(Guid workoutId) => DeleteHandler(workoutId);

    public void Reset()
    {
        GetAllHandler = () => Task.FromResult(ResultOfT<IReadOnlyList<WorkoutsListItemResponse>>.Success(
            new List<WorkoutsListItemResponse>
            {
                new("Strength", 60, 5, new DateTime(2026, 4, 20))
            }));

        GetByIdHandler = id => Task.FromResult(ResultOfT<WorkoutsResponse>.Success(
            new WorkoutsResponse(
                id,
                new DateTime(2026, 4, 20),
                60,
                "Test notes",
                new WorkoutTypeResponse(Guid.NewGuid(), "Strength", null),
                new List<ExerciseResponse>())));

        CreateHandler = _ => Task.FromResult(ResultOfT<CreateWorkoutResponse>.Success(
            new CreateWorkoutResponse(Guid.NewGuid())));

        UpdateHandler = _ => Task.FromResult(Result.Success());
        DeleteHandler = _ => Task.FromResult(Result.Success());
    }
}
