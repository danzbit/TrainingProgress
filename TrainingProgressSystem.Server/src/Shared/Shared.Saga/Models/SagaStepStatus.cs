namespace Shared.Saga.Models;

public enum SagaStepStatus
{
    Pending = 0,
    Succeeded = 1,
    Failed = 2,
    Compensated = 3,
    Skipped = 4
}
