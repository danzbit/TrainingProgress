using Bff.Application.Interfaces.v1;
using Bff.Infrastructure.Helpers;
using Shared.Grpc.Contracts;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using Shared.Saga.Models;

namespace Bff.Infrastructure.Clients;

internal sealed class AnalyticsSyncGrpcClient(AnalyticsSyncGrpc.AnalyticsSyncGrpcClient client)
    : IAnalyticsSyncClient
{
    public async Task<Result> RecalculateForWorkoutAsync(Guid userId, Guid workoutId, SagaCallContext context)
    {
        var response = await client.RecalculateForWorkoutAsync(
            new RecalculateForWorkoutGrpcRequest
            {
                UserId = userId.ToString(),
                WorkoutId = workoutId.ToString()
            },
            GrpcCallContextHelper.BuildHeaders(context),
            GrpcCallContextHelper.BuildDeadline(context),
            context.CancellationToken);

        return response.IsSuccess
            ? Result.Success()
            : Result.Failure(new Error(ErrorCode.DownstreamServiceUnavailable, response.Error));
    }
}
