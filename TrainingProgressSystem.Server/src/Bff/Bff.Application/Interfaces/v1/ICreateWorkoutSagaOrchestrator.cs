using Bff.Application.Dtos.v1.Requests;
using Bff.Application.Dtos.v1.Responses;
using Shared.Kernal.Results;

namespace Bff.Application.Interfaces.v1;

public interface ICreateWorkoutSagaOrchestrator
{
    Task<ResultOfT<CreateWorkoutSagaResult>> ExecuteAsync(
        CreateWorkoutSagaCommand command,
        string? idempotencyKey = null,
        CancellationToken ct = default);
}
