using System.Net;
using Bff.Api.Controllers.v1;
using Bff.Application.Dtos.v1.Requests;
using Bff.Application.Dtos.v1.Responses;
using Bff.Application.Interfaces.v1;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Kernal.Errors;
using Shared.Kernal.Headers;
using Shared.Kernal.Results;

namespace Bff.Api.Tests.Controllers.v1;

[TestFixture]
public class GoalOrchestrationControllerTests
{
    private Mock<ISaveGoalSagaOrchestrator> _orchestratorMock = null!;
    private GoalOrchestrationController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _orchestratorMock = new Mock<ISaveGoalSagaOrchestrator>(MockBehavior.Strict);
        _controller = new GoalOrchestrationController(_orchestratorMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };
    }

    [Test]
    public async Task SaveGoalAndPropagate_WhenOrchestratorSucceeds_ReturnsOkWithGoalIdAndIdempotencyKey()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var idempotencyKey = "goal-key-123";
        var command = new SaveGoalSagaCommand(
            "Run 5k",
            "Weekly endurance goal",
            1,
            2,
            5,
            new DateTime(2026, 4, 22, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 22, 0, 0, 0, DateTimeKind.Utc),
            Guid.NewGuid());

        _orchestratorMock
            .Setup(o => o.ExecuteAsync(command, idempotencyKey, CancellationToken.None))
            .ReturnsAsync(ResultOfT<SaveGoalSagaResult>.Success(
                new SaveGoalSagaResult(goalId, null)));

        _controller.ControllerContext.HttpContext.Request.Headers[IdempotencyHeaders.IdempotencyKey] = idempotencyKey;

        // Act
        var result = await _controller.SaveGoalAndPropagate(command, CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));

        dynamic value = okResult.Value!;
        Assert.That((Guid)value.goalId, Is.EqualTo(goalId));
        Assert.That((string?)value.error, Is.Null);

        _orchestratorMock.Verify(
            o => o.ExecuteAsync(command, idempotencyKey, CancellationToken.None),
            Times.Once);
    }

    [Test]
    public async Task SaveGoalAndPropagate_WhenOrchestratorFails_ReturnsBadRequestWithGoalIdAndError()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var command = new SaveGoalSagaCommand(
            "Run 5k",
            "Weekly endurance goal",
            1,
            2,
            5,
            new DateTime(2026, 4, 22, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 22, 0, 0, 0, DateTimeKind.Utc),
            Guid.NewGuid());

        var errorMessage = "Goal propagation failed";
        _orchestratorMock
            .Setup(o => o.ExecuteAsync(command, null, CancellationToken.None))
            .ReturnsAsync(ResultOfT<SaveGoalSagaResult>.Failure(
                new SaveGoalSagaResult(goalId, errorMessage),
                new Error(ErrorCode.UnexpectedError, "Unexpected failure")));

        // Act
        var result = await _controller.SaveGoalAndPropagate(command, CancellationToken.None);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));

        dynamic value = badRequestResult.Value!;
        Assert.That((Guid)value.goalId, Is.EqualTo(goalId));
        Assert.That((string)value.error, Is.EqualTo(errorMessage));

        _orchestratorMock.Verify(
            o => o.ExecuteAsync(command, null, CancellationToken.None),
            Times.Once);
    }

    [Test]
    public async Task SaveGoalAndPropagate_WhenOrchestratorThrowsException_ReturnsBadRequestWithErrorDescription()
    {
        // Arrange
        var command = new SaveGoalSagaCommand(
            "Run 5k",
            "Weekly endurance goal",
            1,
            2,
            5,
            new DateTime(2026, 4, 22, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 22, 0, 0, 0, DateTimeKind.Utc),
            Guid.NewGuid());

        var errorDescription = "Database connection failed";
        _orchestratorMock
            .Setup(o => o.ExecuteAsync(command, null, CancellationToken.None))
            .ReturnsAsync(ResultOfT<SaveGoalSagaResult>.Failure(
                new Error(ErrorCode.UnexpectedError, errorDescription)));

        // Act
        var result = await _controller.SaveGoalAndPropagate(command, CancellationToken.None);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);

        dynamic value = badRequestResult.Value!;
        Assert.That((string)value.error, Is.EqualTo(errorDescription));
    }

    [Test]
    public async Task SaveGoalAndPropagate_WhenNoIdempotencyKeyProvided_PassesNullToOrchestrator()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var command = new SaveGoalSagaCommand(
            "Run 5k",
            "Weekly endurance goal",
            1,
            2,
            5,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            Guid.NewGuid());

        _orchestratorMock
            .Setup(o => o.ExecuteAsync(command, null, CancellationToken.None))
            .ReturnsAsync(ResultOfT<SaveGoalSagaResult>.Success(
                new SaveGoalSagaResult(goalId, null)));

        // Act
        var result = await _controller.SaveGoalAndPropagate(command, CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        _orchestratorMock.Verify(
            o => o.ExecuteAsync(command, null, CancellationToken.None),
            Times.Once);
    }
}
