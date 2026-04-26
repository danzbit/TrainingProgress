using Bff.Application.Dtos.v1.Requests;
using Bff.Application.Dtos.v1.Responses;
using Bff.Application.Interfaces.v1;
using Shared.Kernal.Results;

namespace Bff.Api.Integration.Infrastructure;

public sealed class StubSaveGoalSagaOrchestrator : ISaveGoalSagaOrchestrator
{
    public Func<SaveGoalSagaCommand, string?, CancellationToken, Task<ResultOfT<SaveGoalSagaResult>>> Handler { get; set; } =
        (_, _, _) => Task.FromResult(ResultOfT<SaveGoalSagaResult>.Success(new SaveGoalSagaResult(Guid.NewGuid())));

    public SaveGoalSagaCommand? LastCommand { get; private set; }

    public string? LastIdempotencyKey { get; private set; }

    public int CallCount { get; private set; }

    public Task<ResultOfT<SaveGoalSagaResult>> ExecuteAsync(
        SaveGoalSagaCommand command,
        string? idempotencyKey = null,
        CancellationToken ct = default)
    {
        CallCount++;
        LastCommand = command;
        LastIdempotencyKey = idempotencyKey;
        return Handler(command, idempotencyKey, ct);
    }

    public void Reset()
    {
        Handler = (_, _, _) => Task.FromResult(ResultOfT<SaveGoalSagaResult>.Success(new SaveGoalSagaResult(Guid.NewGuid())));
        LastCommand = null;
        LastIdempotencyKey = null;
        CallCount = 0;
    }
}