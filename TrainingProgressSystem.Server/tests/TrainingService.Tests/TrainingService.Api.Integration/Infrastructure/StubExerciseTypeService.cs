using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;

namespace TrainingService.Api.Integration.Infrastructure;

public sealed class StubExerciseTypeService : IExerciseTypeService
{
    public Func<Task<ResultOfT<IReadOnlyList<ExerciseTypeResponse>>>> GetAllHandler { get; set; } = default!;

    public StubExerciseTypeService() => Reset();

    public Task<ResultOfT<IReadOnlyList<ExerciseTypeResponse>>> GetAllExerciseTypesAsync() => GetAllHandler();

    public void Reset()
    {
        GetAllHandler = () => Task.FromResult(ResultOfT<IReadOnlyList<ExerciseTypeResponse>>.Success(
            new List<ExerciseTypeResponse>
            {
                new(Guid.NewGuid(), "Bench Press", "Strength"),
                new(Guid.NewGuid(), "Running", "Cardio")
            }));
    }
}
