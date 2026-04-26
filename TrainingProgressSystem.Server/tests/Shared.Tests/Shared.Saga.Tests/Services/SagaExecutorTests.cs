using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using Shared.Saga.Models;
using Shared.Saga.Services;

namespace Shared.Saga.Tests.Services;

[TestFixture]
public class SagaExecutorTests
{
    [Test]
    public async Task ExecuteStepAsync_Generic_WhenStepSucceeds_RecordsSucceededAndReturnsValue()
    {
        var sut = new SagaExecutor();

        var result = await sut.ExecuteStepAsync(
            name: "step-1",
            required: true,
            action: () => Task.FromResult(ResultOfT<int>.Success(7)));

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.EqualTo(7));
        Assert.That(sut.HasCriticalFailure, Is.False);
        Assert.That(sut.Steps.Count, Is.EqualTo(1));
        Assert.That(sut.Steps[0], Is.EqualTo(new SagaStepResult("step-1", SagaStepStatus.Succeeded, true, null)));
    }

    [Test]
    public async Task ExecuteStepAsync_NonGeneric_WhenRequiredStepFails_MarksCriticalFailure()
    {
        var sut = new SagaExecutor();
        var failure = new Error(ErrorCode.ValidationFailed, "required failed");

        var result = await sut.ExecuteStepAsync(
            name: "required-step",
            required: true,
            action: () => Task.FromResult(Result.Failure(failure)));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(failure));
        Assert.That(sut.HasCriticalFailure, Is.True);
        Assert.That(sut.Steps[^1], Is.EqualTo(new SagaStepResult("required-step", SagaStepStatus.Failed, true, "required failed")));
    }

    [Test]
    public async Task ExecuteStepAsync_NonGeneric_WhenOptionalStepFails_DoesNotMarkCriticalFailure()
    {
        var sut = new SagaExecutor();
        var failure = new Error(ErrorCode.UnexpectedError, "optional failed");

        var result = await sut.ExecuteStepAsync(
            name: "optional-step",
            required: false,
            action: () => Task.FromResult(Result.Failure(failure)));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(sut.HasCriticalFailure, Is.False);
        Assert.That(sut.Steps[^1], Is.EqualTo(new SagaStepResult("optional-step", SagaStepStatus.Failed, false, "optional failed")));
    }

    [Test]
    public async Task ExecuteStepAsync_AfterCriticalFailure_SkipsSubsequentSteps()
    {
        var sut = new SagaExecutor();

        await sut.ExecuteStepAsync(
            name: "critical",
            required: true,
            action: () => Task.FromResult(Result.Failure(new Error(ErrorCode.ValidationFailed, "boom"))));

        var wasCalled = false;
        var skippedResult = await sut.ExecuteStepAsync(
            name: "later",
            required: false,
            action: () =>
            {
                wasCalled = true;
                return Task.FromResult(Result.Success());
            });

        Assert.That(wasCalled, Is.False);
        Assert.That(skippedResult.IsFailure, Is.True);
        Assert.That(skippedResult.Error.Description, Is.EqualTo("Skipped due to previous critical failure."));
        Assert.That(sut.Steps[^1], Is.EqualTo(new SagaStepResult("later", SagaStepStatus.Skipped, false, "Skipped due to previous critical failure.")));
    }

    [Test]
    public async Task CompensateAsync_WhenNoCompensationsRegistered_ReturnsSuccessAndNoStepsAdded()
    {
        var sut = new SagaExecutor();

        var result = await sut.CompensateAsync();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(sut.Steps, Is.Empty);
    }

    [Test]
    public async Task CompensateAsync_ExecutesCompensationsInReverseOrder_AndRecordsCompensatedSteps()
    {
        var sut = new SagaExecutor();
        var executionOrder = new List<string>();

        await sut.ExecuteStepAsync(
            name: "step-a",
            required: true,
            action: () => Task.FromResult(Result.Success()),
            compensation: () =>
            {
                executionOrder.Add("step-a");
                return Task.FromResult(Result.Success());
            });

        await sut.ExecuteStepAsync(
            name: "step-b",
            required: true,
            action: () => Task.FromResult(Result.Success()),
            compensation: () =>
            {
                executionOrder.Add("step-b");
                return Task.FromResult(Result.Success());
            });

        var compensationResult = await sut.CompensateAsync();

        Assert.That(compensationResult.IsFailure, Is.False);
        Assert.That(executionOrder, Is.EqualTo(new[] { "step-b", "step-a" }));
        Assert.That(sut.Steps[^2], Is.EqualTo(new SagaStepResult("step-b.compensation", SagaStepStatus.Compensated, true, null)));
        Assert.That(sut.Steps[^1], Is.EqualTo(new SagaStepResult("step-a.compensation", SagaStepStatus.Compensated, true, null)));
    }

    [Test]
    public async Task CompensateAsync_WhenCompensationFails_StopsAndRecordsFailure()
    {
        var sut = new SagaExecutor();
        var compensationCalls = 0;

        await sut.ExecuteStepAsync(
            name: "step-ok",
            required: true,
            action: () => Task.FromResult(Result.Success()),
            compensation: () =>
            {
                compensationCalls++;
                return Task.FromResult(Result.Success());
            });

        await sut.ExecuteStepAsync(
            name: "step-fail",
            required: true,
            action: () => Task.FromResult(Result.Success()),
            compensation: () =>
            {
                compensationCalls++;
                return Task.FromResult(Result.Failure(new Error(ErrorCode.UnexpectedError, "compensation failed")));
            });

        var result = await sut.CompensateAsync();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Description, Is.EqualTo("compensation failed"));
        Assert.That(compensationCalls, Is.EqualTo(1));
        Assert.That(sut.Steps[^1], Is.EqualTo(new SagaStepResult("step-fail.compensation", SagaStepStatus.Failed, true, "compensation failed")));
    }
}