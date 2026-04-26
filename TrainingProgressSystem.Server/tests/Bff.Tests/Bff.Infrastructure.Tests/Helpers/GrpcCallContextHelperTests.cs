using Bff.Infrastructure.Helpers;
using Grpc.Core;
using Shared.Saga.Models;

namespace Bff.Infrastructure.Tests.Helpers;

[TestFixture]
public class GrpcCallContextHelperTests
{
    [Test]
    public void BuildHeaders_WithValidContext_ReturnsMetadataWithCorrelationIdAndIdempotencyKey()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var idempotencyKey = "test-idempotency-key";
        var context = new SagaCallContext(
            CorrelationId: correlationId,
            IdempotencyKey: idempotencyKey,
            StepTimeout: TimeSpan.FromSeconds(30),
            CancellationToken: CancellationToken.None);

        // Act
        var headers = GrpcCallContextHelper.BuildHeaders(context);

        // Assert
        Assert.That(headers, Is.Not.Null);
        Assert.That(headers.Count, Is.EqualTo(2));

        var correlationIdEntry = headers.FirstOrDefault(e => e.Key == "x-correlation-id");
        var idempotencyKeyEntry = headers.FirstOrDefault(e => e.Key == "x-idempotency-key");

        Assert.That(correlationIdEntry, Is.Not.Null);
        Assert.That(correlationIdEntry!.Value, Is.EqualTo(correlationId.ToString()));

        Assert.That(idempotencyKeyEntry, Is.Not.Null);
        Assert.That(idempotencyKeyEntry!.Value, Is.EqualTo(idempotencyKey));
    }

    [Test]
    public void BuildHeaders_WithEmptyIdempotencyKey_ReturnsMetadataWithEmptyValue()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var context = new SagaCallContext(
            CorrelationId: correlationId,
            IdempotencyKey: string.Empty,
            StepTimeout: TimeSpan.FromSeconds(30),
            CancellationToken: CancellationToken.None);

        // Act
        var headers = GrpcCallContextHelper.BuildHeaders(context);

        // Assert
        var idempotencyKeyEntry = headers.FirstOrDefault(e => e.Key == "x-idempotency-key");
        Assert.That(idempotencyKeyEntry!.Value, Is.EqualTo(string.Empty));
    }

    [Test]
    public void BuildHeaders_WithNullIdempotencyKey_ThrowsArgumentNullException()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var context = new SagaCallContext(
            CorrelationId: correlationId,
            IdempotencyKey: null!,
            StepTimeout: TimeSpan.FromSeconds(30),
            CancellationToken: CancellationToken.None);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => GrpcCallContextHelper.BuildHeaders(context));
    }

    [Test]
    public void BuildDeadline_WithValidContext_ReturnsUtcNowPlusSagaTimeout()
    {
        // Arrange
        var context = new SagaCallContext(
            CorrelationId: Guid.NewGuid(),
            IdempotencyKey: "test-key",
            StepTimeout: TimeSpan.FromSeconds(30),
            CancellationToken: CancellationToken.None);

        var beforeCall = DateTime.UtcNow.Add(TimeSpan.FromSeconds(30));

        // Act
        var deadline = GrpcCallContextHelper.BuildDeadline(context);

        var afterCall = DateTime.UtcNow.Add(TimeSpan.FromSeconds(30));

        // Assert
        Assert.That(deadline, Is.GreaterThanOrEqualTo(beforeCall));
        Assert.That(deadline, Is.LessThanOrEqualTo(afterCall));
    }

    [Test]
    public void BuildDeadline_WithDifferentTimeouts_ReturnsCorrectDeadlineValues()
    {
        // Arrange
        var timeout1 = TimeSpan.FromSeconds(10);
        var context1 = new SagaCallContext(
            CorrelationId: Guid.NewGuid(),
            IdempotencyKey: "key1",
            StepTimeout: timeout1,
            CancellationToken: CancellationToken.None);

        var timeout2 = TimeSpan.FromSeconds(60);
        var context2 = new SagaCallContext(
            CorrelationId: Guid.NewGuid(),
            IdempotencyKey: "key2",
            StepTimeout: timeout2,
            CancellationToken: CancellationToken.None);

        var before1 = DateTime.UtcNow.Add(timeout1);
        var deadline1 = GrpcCallContextHelper.BuildDeadline(context1);
        var after1 = DateTime.UtcNow.Add(timeout1);

        var before2 = DateTime.UtcNow.Add(timeout2);
        var deadline2 = GrpcCallContextHelper.BuildDeadline(context2);
        var after2 = DateTime.UtcNow.Add(timeout2);

        // Assert
        Assert.That(deadline1, Is.GreaterThanOrEqualTo(before1).And.LessThanOrEqualTo(after1));
        Assert.That(deadline2, Is.GreaterThanOrEqualTo(before2).And.LessThanOrEqualTo(after2));
        Assert.That(deadline2, Is.GreaterThan(deadline1));
    }

    [Test]
    public void BuildDeadline_WithZeroTimeout_ReturnsCurrentUtcTime()
    {
        // Arrange
        var context = new SagaCallContext(
            CorrelationId: Guid.NewGuid(),
            IdempotencyKey: "test-key",
            StepTimeout: TimeSpan.Zero,
            CancellationToken: CancellationToken.None);

        var before = DateTime.UtcNow;

        // Act
        var deadline = GrpcCallContextHelper.BuildDeadline(context);

        var after = DateTime.UtcNow;

        // Assert
        Assert.That(deadline, Is.GreaterThanOrEqualTo(before).And.LessThanOrEqualTo(after.AddMilliseconds(10)));
    }
}
