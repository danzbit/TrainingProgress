namespace Shared.Saga.Models;

public sealed record SagaCallContext(
    Guid CorrelationId,
    string IdempotencyKey,
    TimeSpan StepTimeout,
    CancellationToken CancellationToken
);
