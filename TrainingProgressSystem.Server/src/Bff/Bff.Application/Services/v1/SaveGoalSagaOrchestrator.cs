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

public sealed class SaveGoalSagaOrchestrator(
    ITrainingSyncClient trainingSyncClient,
    INotificationSyncClient notificationSyncClient,
    ISyncNotifier syncNotifier,
    ICurrentUser currentUser,
    IOptions<SaveGoalSagaOptions> options,
    ILogger<SaveGoalSagaOrchestrator> logger) : ISaveGoalSagaOrchestrator
{
    public async Task<ResultOfT<SaveGoalSagaResult>> ExecuteAsync(
        SaveGoalSagaCommand command,
        string? idempotencyKey = null,
        CancellationToken ct = default)
    {
        var userIdResult = currentUser.GetCurrentUserId();
        if (userIdResult.IsFailure)
        {
            return ResultOfT<SaveGoalSagaResult>.Failure(userIdResult.Error);
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
            ct);

        var saga = new SagaExecutor();

        logger.LogInformation("Starting SaveGoal saga. CorrelationId: {CorrelationId}", correlationId);

        Guid? createdGoalId = null;

        var saveGoalResult = await saga.ExecuteStepAsync(
            OrchestrationStepNames.SaveGoal,
            required: true,
            action: async () =>
            {
                var result = await trainingSyncClient.SaveGoalAsync(command.ToSaveGoalCommand(userId), context);
                if (!result.IsFailure)
                {
                    createdGoalId = result.Value;
                }

                return result;
            },
            compensation: async () =>
            {
                if (!createdGoalId.HasValue)
                {
                    return Result.Failure(BffApplicationErrors.CompensationGoalIdMissing());
                }

                return await trainingSyncClient.DeleteGoalAsync(createdGoalId.Value, context);
            });

        Guid? goalId = null;
        if (!saveGoalResult.IsFailure)
        {
            goalId = saveGoalResult.Value;
        }

        if (goalId.HasValue)
        {
            await saga.ExecuteStepAsync(
                OrchestrationStepNames.RecalculateGoalProgress,
                required: false,
                () => trainingSyncClient.RecalculateProgressForGoalAsync(userId, goalId.Value, context));

            await saga.ExecuteStepAsync(
                OrchestrationStepNames.ScheduleGoalReminders,
                options.Value.NotificationRequired,
                () => notificationSyncClient.ScheduleRemindersForGoalAsync(userId, goalId.Value, context));
        }

        if (saga.HasCriticalFailure && options.Value.CompensateOnCriticalFailure)
        {
            logger.LogWarning("Critical failure in SaveGoal saga. Running compensation. CorrelationId: {CorrelationId}",
                correlationId);
            var compensationResult = await saga.CompensateAsync();
            if (compensationResult.IsFailure)
            {
                logger.LogError("Saga compensation failed. CorrelationId: {CorrelationId}, Error: {Error}",
                    correlationId,
                    compensationResult.Error.Description);
            }

            goalId = null;
        }

        var resultPayload = new SaveGoalSagaResult(goalId);

        if (saga.HasCriticalFailure)
        {
            var failureMessage = saga.Steps.LastOrDefault(step => step.Status == SagaStepStatus.Failed && step.Required)?.Error
                                 ?? "SaveGoal saga failed.";

            logger.LogWarning("SaveGoal saga finished with critical failure. CorrelationId: {CorrelationId}", correlationId);
            return ResultOfT<SaveGoalSagaResult>.Failure(
                resultPayload with { Error = failureMessage },
                BffApplicationErrors.SagaCriticalStepFailed(failureMessage));
        }

        logger.LogInformation("SaveGoal saga completed successfully. CorrelationId: {CorrelationId}", correlationId);

        if (goalId.HasValue)
        {
            await syncNotifier.NotifyGoalSavedAsync(userId, goalId.Value, ct);
        }

        return ResultOfT<SaveGoalSagaResult>.Success(resultPayload);
    }
}
