using Bff.Application.Dtos.v1.Requests;
using Bff.Application.Dtos.v1.Responses;
using Shared.Kernal.Results;

namespace Bff.Application.Interfaces.v1;

public interface ISaveGoalSagaOrchestrator
{
    Task<ResultOfT<SaveGoalSagaResult>> ExecuteAsync(
        SaveGoalSagaCommand command,
        string? idempotencyKey = null,
        CancellationToken ct = default);
}
