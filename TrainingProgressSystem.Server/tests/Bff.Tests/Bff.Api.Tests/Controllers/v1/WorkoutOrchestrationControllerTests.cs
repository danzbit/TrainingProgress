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
public class WorkoutOrchestrationControllerTests
{
    private Mock<ICreateWorkoutSagaOrchestrator> _orchestratorMock = null!;
    private WorkoutOrchestrationController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _orchestratorMock = new Mock<ICreateWorkoutSagaOrchestrator>(MockBehavior.Strict);
        _controller = new WorkoutOrchestrationController(_orchestratorMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };
    }

    [Test]
    public async Task CreateWorkoutAndPropagate_WhenOrchestratorSucceeds_ReturnsOkWithWorkoutIdAndIdempotencyKey()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var idempotencyKey = "workout-key-123";
        var command = new CreateWorkoutSagaCommand(
            new DateTime(2026, 4, 23, 8, 0, 0, DateTimeKind.Utc),
            Guid.NewGuid(),
            30,
            "5km run in the park",
            null,
            Guid.NewGuid());

        _orchestratorMock
            .Setup(o => o.ExecuteAsync(command, idempotencyKey, CancellationToken.None))
            .ReturnsAsync(ResultOfT<CreateWorkoutSagaResult>.Success(
                new CreateWorkoutSagaResult(workoutId, null)));

        _controller.ControllerContext.HttpContext.Request.Headers[IdempotencyHeaders.IdempotencyKey] = idempotencyKey;

        // Act
        var result = await _controller.CreateWorkoutAndPropagate(command, CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));

        dynamic value = okResult.Value!;
        Assert.That((Guid)value.workoutId, Is.EqualTo(workoutId));
        Assert.That((string?)value.error, Is.Null);

        _orchestratorMock.Verify(
            o => o.ExecuteAsync(command, idempotencyKey, CancellationToken.None),
            Times.Once);
    }

    [Test]
    public async Task CreateWorkoutAndPropagate_WhenOrchestratorFails_ReturnsBadRequestWithWorkoutIdAndError()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var command = new CreateWorkoutSagaCommand(
            new DateTime(2026, 4, 23, 8, 0, 0, DateTimeKind.Utc),
            Guid.NewGuid(),
            30,
            "5km run in the park",
            null,
            Guid.NewGuid());

        var errorMessage = "Workout propagation failed";
        _orchestratorMock
            .Setup(o => o.ExecuteAsync(command, null, CancellationToken.None))
            .ReturnsAsync(ResultOfT<CreateWorkoutSagaResult>.Failure(
                new CreateWorkoutSagaResult(workoutId, errorMessage),
                new Error(ErrorCode.UnexpectedError, "Unexpected failure")));

        // Act
        var result = await _controller.CreateWorkoutAndPropagate(command, CancellationToken.None);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));

        dynamic value = badRequestResult.Value!;
        Assert.That((Guid)value.workoutId, Is.EqualTo(workoutId));
        Assert.That((string)value.error, Is.EqualTo(errorMessage));

        _orchestratorMock.Verify(
            o => o.ExecuteAsync(command, null, CancellationToken.None),
            Times.Once);
    }

    [Test]
    public async Task CreateWorkoutAndPropagate_WhenOrchestratorThrowsException_ReturnsBadRequestWithErrorDescription()
    {
        // Arrange
        var command = new CreateWorkoutSagaCommand(
            new DateTime(2026, 4, 23, 8, 0, 0, DateTimeKind.Utc),
            Guid.NewGuid(),
            30,
            "5km run in the park",
            null,
            Guid.NewGuid());

        var errorDescription = "Database connection failed";
        _orchestratorMock
            .Setup(o => o.ExecuteAsync(command, null, CancellationToken.None))
            .ReturnsAsync(ResultOfT<CreateWorkoutSagaResult>.Failure(
                new Error(ErrorCode.UnexpectedError, errorDescription)));

        // Act
        var result = await _controller.CreateWorkoutAndPropagate(command, CancellationToken.None);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);

        dynamic value = badRequestResult.Value!;
        Assert.That((string)value.error, Is.EqualTo(errorDescription));
    }

    [Test]
    public async Task CreateWorkoutAndPropagate_WhenNoIdempotencyKeyProvided_PassesNullToOrchestrator()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        var command = new CreateWorkoutSagaCommand(
            DateTime.UtcNow,
            Guid.NewGuid(),
            30,
            "5km run in the park",
            null,
            Guid.NewGuid());

        _orchestratorMock
            .Setup(o => o.ExecuteAsync(command, null, CancellationToken.None))
            .ReturnsAsync(ResultOfT<CreateWorkoutSagaResult>.Success(
                new CreateWorkoutSagaResult(workoutId, null)));

        // Act
        var result = await _controller.CreateWorkoutAndPropagate(command, CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        _orchestratorMock.Verify(
            o => o.ExecuteAsync(command, null, CancellationToken.None),
            Times.Once);
    }
}
