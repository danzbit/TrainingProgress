using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using Shared.Saga.Models;

namespace Shared.Saga.Services;

public sealed class SagaExecutor
{
    private readonly List<SagaStepResult> _steps = [];
    private readonly Stack<(string Name, Func<Task<Result>> Compensation)> _compensations = new();

    public IReadOnlyList<SagaStepResult> Steps => _steps;

    public bool HasCriticalFailure { get; private set; }

    public async Task<ResultOfT<T>> ExecuteStepAsync<T>(
        string name,
        bool required,
        Func<Task<ResultOfT<T>>> action,
        Func<Task<Result>>? compensation = null)
    {
        if (HasCriticalFailure)
        {
            _steps.Add(new SagaStepResult(name, SagaStepStatus.Skipped, required, "Skipped due to previous critical failure."));
            return ResultOfT<T>.Failure(new Error(ErrorCode.UnexpectedError, "Skipped due to previous critical failure."));
        }

        var result = await action();
        if (!result.IsFailure)
        {
            _steps.Add(new SagaStepResult(name, SagaStepStatus.Succeeded, required));
            if (compensation != null)
            {
                _compensations.Push((name, compensation));
            }

            return result;
        }

        _steps.Add(new SagaStepResult(name, SagaStepStatus.Failed, required, result.Error.Description));

        if (required)
        {
            HasCriticalFailure = true;
        }

        return result;
    }

    public async Task<Result> ExecuteStepAsync(
        string name,
        bool required,
        Func<Task<Result>> action,
        Func<Task<Result>>? compensation = null)
    {
        if (HasCriticalFailure)
        {
            _steps.Add(new SagaStepResult(name, SagaStepStatus.Skipped, required, "Skipped due to previous critical failure."));
            return Result.Failure(new Error(ErrorCode.UnexpectedError, "Skipped due to previous critical failure."));
        }

        var result = await action();
        if (!result.IsFailure)
        {
            _steps.Add(new SagaStepResult(name, SagaStepStatus.Succeeded, required));
            if (compensation != null)
            {
                _compensations.Push((name, compensation));
            }

            return result;
        }

        _steps.Add(new SagaStepResult(name, SagaStepStatus.Failed, required, result.Error.Description));

        if (required)
        {
            HasCriticalFailure = true;
        }

        return result;
    }

    public async Task<Result> CompensateAsync()
    {
        while (_compensations.Count > 0)
        {
            var (name, compensation) = _compensations.Pop();
            var result = await compensation();
            if (result.IsFailure)
            {
                _steps.Add(new SagaStepResult($"{name}.compensation", SagaStepStatus.Failed, true, result.Error.Description));
                return result;
            }

            _steps.Add(new SagaStepResult($"{name}.compensation", SagaStepStatus.Compensated, true));
        }

        return Result.Success();
    }
}
