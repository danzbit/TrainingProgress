using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;
using TrainingService.Domain.Enums;

namespace TrainingService.Api.Integration.Infrastructure;

public sealed class StubGoalService : IGoalService
{
    public Func<Task<ResultOfT<IReadOnlyList<GoalsListItemResponse>>>> GetAllHandler { get; set; } = default!;
    public Func<Guid, Task<ResultOfT<GoalsResponse>>> GetByIdHandler { get; set; } = default!;
    public Func<CreateGoalRequest, CancellationToken, Task<ResultOfT<Guid>>> CreateHandler { get; set; } = default!;
    public Func<UpdateGoalRequest, CancellationToken, Task<Result>> UpdateHandler { get; set; } = default!;
    public Func<Guid, CancellationToken, Task<Result>> DeleteHandler { get; set; } = default!;
    public Func<Guid, Guid, CancellationToken, Task<Result>> UpdateGoalsForWorkoutHandler { get; set; } = default!;
    public Func<Guid, Guid, CancellationToken, Task<Result>> RecalculateProgressHandler { get; set; } = default!;

    public StubGoalService() => Reset();

    public Task<ResultOfT<IReadOnlyList<GoalsListItemResponse>>> GetAllGoalsAsync() => GetAllHandler();
    public Task<ResultOfT<GoalsResponse>> GetGoalAsync(Guid goalId) => GetByIdHandler(goalId);
    public Task<ResultOfT<Guid>> CreateGoalAsync(CreateGoalRequest request, CancellationToken ct = default) => CreateHandler(request, ct);
    public Task<Result> UpdateGoalAsync(UpdateGoalRequest request, CancellationToken ct = default) => UpdateHandler(request, ct);
    public Task<Result> DeleteGoalAsync(Guid goalId, CancellationToken ct = default) => DeleteHandler(goalId, ct);
    public Task<Result> UpdateGoalsForWorkoutAsync(Guid userId, Guid workoutId, CancellationToken ct = default) => UpdateGoalsForWorkoutHandler(userId, workoutId, ct);
    public Task<Result> RecalculateProgressForGoalAsync(Guid userId, Guid goalId, CancellationToken ct = default) => RecalculateProgressHandler(userId, goalId, ct);

    public void Reset()
    {
        var progressInfo = new GoalProgressInfoResponse(3, 60.0, false, DateTime.UtcNow);

        GetAllHandler = () => Task.FromResult(ResultOfT<IReadOnlyList<GoalsListItemResponse>>.Success(
            new List<GoalsListItemResponse>
            {
                new(Guid.NewGuid(), "Run 5km weekly", "Cardio goal",
                    GoalMetricType.WorkoutCount, GoalPeriodType.Weekly,
                    GoalStatus.Active, 5, DateTime.UtcNow.AddDays(-10), null, progressInfo)
            }));

        GetByIdHandler = id => Task.FromResult(ResultOfT<GoalsResponse>.Success(
            new GoalsResponse(
                id, Guid.NewGuid(), "Run 5km weekly", "Cardio goal",
                GoalMetricType.WorkoutCount, GoalPeriodType.Weekly, 5,
                GoalStatus.Active, DateTime.UtcNow.AddDays(-10), null,
                3, 60.0, false, DateTime.UtcNow)));

        CreateHandler = (_, _) => Task.FromResult(ResultOfT<Guid>.Success(Guid.NewGuid()));
        UpdateHandler = (_, _) => Task.FromResult(Result.Success());
        DeleteHandler = (_, _) => Task.FromResult(Result.Success());
        UpdateGoalsForWorkoutHandler = (_, _, _) => Task.FromResult(Result.Success());
        RecalculateProgressHandler = (_, _, _) => Task.FromResult(Result.Success());
    }
}
