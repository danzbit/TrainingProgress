using Grpc.Core;
using Shared.Saga.Models;

namespace Bff.Infrastructure.Helpers;

internal static class GrpcCallContextHelper
{
    public static Metadata BuildHeaders(SagaCallContext context)
    {
        return
        [
            new Metadata.Entry("x-correlation-id", context.CorrelationId.ToString()),
            new Metadata.Entry("x-idempotency-key", context.IdempotencyKey)
        ];
    }

    public static DateTime BuildDeadline(SagaCallContext context) => DateTime.UtcNow.Add(context.StepTimeout);
}
