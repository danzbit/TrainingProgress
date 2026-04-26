using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;

namespace TrainingService.Api.Integration.Infrastructure;

public sealed class StubWorkoutTypeService : IWorkoutTypeService
{
    public Func<Task<ResultOfT<IReadOnlyList<WorkoutTypeResponse>>>> GetAllHandler { get; set; } = default!;

    public StubWorkoutTypeService() => Reset();

    public Task<ResultOfT<IReadOnlyList<WorkoutTypeResponse>>> GetAllWorkoutTypesAsync() => GetAllHandler();

    public void Reset()
    {
        GetAllHandler = () => Task.FromResult(ResultOfT<IReadOnlyList<WorkoutTypeResponse>>.Success(
            new List<WorkoutTypeResponse>
            {
                new(Guid.NewGuid(), "Strength", "Strength training"),
                new(Guid.NewGuid(), "Cardio", "Cardiovascular training")
            }));
    }
}
