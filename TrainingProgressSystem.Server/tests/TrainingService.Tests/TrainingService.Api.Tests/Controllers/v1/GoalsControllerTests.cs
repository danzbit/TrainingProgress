using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Api.Controllers.v1;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;
using TrainingService.Domain.Enums;

namespace TrainingService.Api.Tests.Controllers.v1;

[TestFixture]
public class GoalsControllerTests
{
    private Mock<IGoalService> _goalServiceMock = null!;
    private GoalsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _goalServiceMock = new Mock<IGoalService>(MockBehavior.Strict);
        _controller = new GoalsController(_goalServiceMock.Object);
    }

    [Test]
    public async Task GetAll_WhenServiceSucceeds_ReturnsOkObjectResultWithQueryable()
    {
        IReadOnlyList<GoalsListItemResponse> goals =
        [
            new GoalsListItemResponse(
                Guid.NewGuid(),
                "Goal",
                "Description",
                GoalMetricType.WorkoutCount,
                GoalPeriodType.Weekly,
                GoalStatus.Active,
                3,
                DateTime.UtcNow.Date,
                null,
                new GoalProgressInfoResponse(1, 33.3, false, DateTime.UtcNow))
        ];

        _goalServiceMock
            .Setup(service => service.GetAllGoalsAsync())
            .ReturnsAsync(ResultOfT<IReadOnlyList<GoalsListItemResponse>>.Success(goals));

        var result = await _controller.GetAll();

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.AssignableTo<IQueryable<GoalsListItemResponse>>());
    }

    [Test]
    public async Task GetAll_WhenServiceFails_ReturnsMappedErrorResult()
    {
        _goalServiceMock
            .Setup(service => service.GetAllGoalsAsync())
            .ReturnsAsync(ResultOfT<IReadOnlyList<GoalsListItemResponse>>.Failure(new Error(ErrorCode.EntityNotFound, "No goals")));

        var result = await _controller.GetAll();

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetById_WhenServiceSucceeds_ReturnsOkObjectResult()
    {
        var goalId = Guid.NewGuid();
        var response = new GoalsResponse(
            goalId,
            Guid.NewGuid(),
            "Goal",
            "Description",
            GoalMetricType.WorkoutCount,
            GoalPeriodType.Monthly,
            8,
            GoalStatus.Active,
            DateTime.UtcNow.Date,
            null,
            2,
            25,
            false,
            DateTime.UtcNow);

        _goalServiceMock
            .Setup(service => service.GetGoalAsync(goalId))
            .ReturnsAsync(ResultOfT<GoalsResponse>.Success(response));

        var result = await _controller.GetById(goalId);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(response));
    }

    [Test]
    public async Task Update_WhenServiceFails_ReturnsBadRequestObjectResult()
    {
        var request = new UpdateGoalRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Goal",
            "Description",
            GoalMetricType.WorkoutCount,
            GoalPeriodType.Weekly,
            5,
            DateTime.UtcNow.Date,
            null);

        _goalServiceMock
            .Setup(service => service.UpdateGoalAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error(ErrorCode.ValidationFailed, "Invalid")));

        var result = await _controller.Update(request);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Delete_WhenServiceSucceeds_ReturnsOkResult()
    {
        var goalId = Guid.NewGuid();

        _goalServiceMock
            .Setup(service => service.DeleteGoalAsync(goalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Delete(goalId);

        Assert.That(result, Is.TypeOf<OkResult>());
    }
}
