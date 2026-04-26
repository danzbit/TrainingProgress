using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Api.Controllers.v1;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;

namespace TrainingService.Api.Tests.Controllers.v1;

[TestFixture]
public class WorkoutsControllerTests
{
    private Mock<IWorkoutService> _workoutServiceMock = null!;
    private WorkoutsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _workoutServiceMock = new Mock<IWorkoutService>(MockBehavior.Strict);
        _controller = new WorkoutsController(_workoutServiceMock.Object);
    }

    [Test]
    public async Task GetAll_WhenServiceSucceeds_ReturnsOkObjectResultWithQueryable()
    {
        IReadOnlyList<WorkoutsListItemResponse> workouts =
        [
            new WorkoutsListItemResponse("Strength", 45, 6, DateTime.UtcNow)
        ];

        _workoutServiceMock
            .Setup(service => service.GetAllWorkoutsAsync())
            .ReturnsAsync(ResultOfT<IReadOnlyList<WorkoutsListItemResponse>>.Success(workouts));

        var result = await _controller.GetAll();

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.AssignableTo<IQueryable<WorkoutsListItemResponse>>());
    }

    [Test]
    public async Task GetById_WhenServiceFailsWithEntityNotFound_ReturnsNotFoundObjectResult()
    {
        var workoutId = Guid.NewGuid();

        _workoutServiceMock
            .Setup(service => service.GetWorkoutAsync(workoutId))
            .ReturnsAsync(ResultOfT<WorkoutsResponse>.Failure(new Error(ErrorCode.EntityNotFound, "Missing")));

        var result = await _controller.GetById(workoutId);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Update_WhenServiceSucceeds_ReturnsOkResult()
    {
        var request = new UpdateWorkoutRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            Guid.NewGuid(),
            50,
            "notes",
            []);

        _workoutServiceMock
            .Setup(service => service.UpdateWorkoutAsync(request))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Update(request);

        Assert.That(result, Is.TypeOf<OkResult>());
    }

    [Test]
    public async Task Delete_WhenServiceFails_ReturnsBadRequestObjectResult()
    {
        var workoutId = Guid.NewGuid();

        _workoutServiceMock
            .Setup(service => service.DeleteWorkoutAsync(workoutId))
            .ReturnsAsync(Result.Failure(new Error(ErrorCode.ValidationFailed, "Cannot delete")));

        var result = await _controller.Delete(workoutId);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }
}
