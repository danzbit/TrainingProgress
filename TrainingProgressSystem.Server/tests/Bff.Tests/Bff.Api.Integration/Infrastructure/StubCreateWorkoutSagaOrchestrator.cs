using Bff.Application.Dtos.v1.Requests;
using Bff.Application.Dtos.v1.Responses;
using Bff.Application.Interfaces.v1;
using Shared.Kernal.Results;

namespace Bff.Api.Integration.Infrastructure;

public sealed class StubCreateWorkoutSagaOrchestrator : ICreateWorkoutSagaOrchestrator
{
    public Func<CreateWorkoutSagaCommand, string?, CancellationToken, Task<ResultOfT<CreateWorkoutSagaResult>>> Handler { get; set; } =
        (_, _, _) => Task.FromResult(ResultOfT<CreateWorkoutSagaResult>.Success(new CreateWorkoutSagaResult(Guid.NewGuid())));

    public CreateWorkoutSagaCommand? LastCommand { get; private set; }

    public string? LastIdempotencyKey { get; private set; }

    public int CallCount { get; private set; }

    public Task<ResultOfT<CreateWorkoutSagaResult>> ExecuteAsync(
        CreateWorkoutSagaCommand command,
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
        Handler = (_, _, _) => Task.FromResult(ResultOfT<CreateWorkoutSagaResult>.Success(new CreateWorkoutSagaResult(Guid.NewGuid())));
        LastCommand = null;
        LastIdempotencyKey = null;
        CallCount = 0;
    }
}