namespace Shared.Saga.Models;

public sealed record SagaStepResult(
    string Name,
    SagaStepStatus Status,
    bool Required,
    string? Error = null
);
