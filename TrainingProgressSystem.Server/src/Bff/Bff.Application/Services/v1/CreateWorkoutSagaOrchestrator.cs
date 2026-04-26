using Bff.Application.Dtos.v1.Commands;
using Bff.Application.Dtos.v1.Requests;
using Bff.Application.Dtos.v1.Responses;
using Bff.Application.Interfaces.v1;
using Bff.Application.Maps.v1;
using Bff.Application.Options.v1;
using Bff.Domain.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Abstractions.Auth;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using Shared.Saga.Models;
using Shared.Saga.Services;

namespace Bff.Application.Services.v1;

public sealed class CreateWorkoutSagaOrchestrator(
    ITrainingSyncClient trainingSyncClient,
    IAnalyticsSyncClient analyticsSyncClient,
    INotificationSyncClient notificationSyncClient,
    ISyncNotifier syncNotifier,
    ICurrentUser currentUser,
    IOptions<CreateWorkoutSagaOptions> options,
    ILogger<CreateWorkoutSagaOrchestrator> logger) : ICreateWorkoutSagaOrchestrator
{
    public async Task<ResultOfT<CreateWorkoutSagaResult>> ExecuteAsync(
        CreateWorkoutSagaCommand command,
        string? idempotencyKey = null,
        CancellationToken ct = default)
    {
        var userIdResult = currentUser.GetCurrentUserId();
        if (userIdResult.IsFailure)
        {
            return ResultOfT<CreateWorkoutSagaResult>.Failure(userIdResult.Error);
        }

        var userId = userIdResult.Value;

        var correlationId = command.CorrelationId ?? Guid.NewGuid();
        var normalizedIdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey)
            ? correlationId.ToString("N")
            : idempotencyKey.Trim();

        var stepTimeoutSeconds = Math.Max(options.Value.StepTimeoutSeconds, 1);
        var context = new SagaCallContext(
            correlationId,
            normalizedIdempotencyKey,
            TimeSpan.FromSeconds(stepTimeoutSeconds),
            CancellationToken.None);

        var saga = new SagaExecutor();

        logger.LogInformation("Starting CreateWorkout saga. CorrelationId: {CorrelationId}", correlationId);

        Guid? createdWorkoutId = null;

        var createResult = await saga.ExecuteStepAsync(
            OrchestrationStepNames.CreateWorkout,
            required: true,
            action: async () =>
            {
                var result = await trainingSyncClient.CreateWorkoutAsync(command.ToCreateWorkoutCommand(userId), context);
                if (!result.IsFailure)
                {
                    createdWorkoutId = result.Value;
                }

                return result;
            },
            compensation: async () =>
            {
                if (!createdWorkoutId.HasValue)
                {
                    return Result.Failure(BffApplicationErrors.CompensationWorkoutIdMissing());
                }

                return await trainingSyncClient.DeleteWorkoutAsync(createdWorkoutId.Value, context);
            });

        Guid? workoutId = null;
        if (!createResult.IsFailure)
        {
            workoutId = createResult.Value;
        }

        if (workoutId.HasValue)
        {
            await saga.ExecuteStepAsync(
                OrchestrationStepNames.RecalculateAnalytics,
                options.Value.AnalyticsRequired,
                () => analyticsSyncClient.RecalculateForWorkoutAsync(userId, workoutId.Value, context));

            await saga.ExecuteStepAsync(
                OrchestrationStepNames.UpdateGoals,
                options.Value.GoalsRequired,
                () => trainingSyncClient.UpdateGoalsForWorkoutAsync(userId, workoutId.Value, context));

            await saga.ExecuteStepAsync(
                OrchestrationStepNames.ResetReminders,
                options.Value.NotificationRequired,
                () => notificationSyncClient.ResetRemindersForWorkoutAsync(userId, workoutId.Value, context));
        }

        if (saga.HasCriticalFailure && options.Value.CompensateOnCriticalFailure)
        {
            logger.LogWarning("Critical failure in CreateWorkout saga. Running compensation. CorrelationId: {CorrelationId}",
                correlationId);
            var compensationResult = await saga.CompensateAsync();
            if (compensationResult.IsFailure)
            {
                logger.LogError("Saga compensation failed. CorrelationId: {CorrelationId}, Error: {Error}",
                    correlationId,
                    compensationResult.Error.Description);
            }

            workoutId = null;
        }

        var resultPayload = new CreateWorkoutSagaResult(workoutId);

        if (saga.HasCriticalFailure)
        {
            var failureMessage = saga.Steps.LastOrDefault(step => step.Status == SagaStepStatus.Failed && step.Required)?.Error
                                 ?? "CreateWorkout saga failed.";

            logger.LogWarning("CreateWorkout saga finished with critical failure. CorrelationId: {CorrelationId}",
                correlationId);
            return ResultOfT<CreateWorkoutSagaResult>.Failure(
                resultPayload with { Error = failureMessage },
                BffApplicationErrors.SagaCriticalStepFailed(failureMessage));
        }

        logger.LogInformation("CreateWorkout saga completed successfully. CorrelationId: {CorrelationId}", correlationId);

        if (workoutId.HasValue)
        {
            await syncNotifier.NotifyWorkoutCreatedAsync(userId, workoutId.Value, ct);
        }

        return ResultOfT<CreateWorkoutSagaResult>.Success(resultPayload);
    }
}
