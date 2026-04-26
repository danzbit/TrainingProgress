using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Api.Controllers.v1;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;

namespace TrainingService.Api.Tests.Controllers.v1;

[TestFixture]
public class WorkoutTypesControllerTests
{
    private Mock<IWorkoutTypeService> _workoutTypeServiceMock = null!;
    private WorkoutTypesController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _workoutTypeServiceMock = new Mock<IWorkoutTypeService>(MockBehavior.Strict);
        _controller = new WorkoutTypesController(_workoutTypeServiceMock.Object);
    }

    [Test]
    public async Task GetAll_WhenServiceSucceeds_ReturnsOkObjectResult()
    {
        IReadOnlyList<WorkoutTypeResponse> response =
        [
            new WorkoutTypeResponse(Guid.NewGuid(), "Strength", "Strength focused")
        ];

        _workoutTypeServiceMock
            .Setup(service => service.GetAllWorkoutTypesAsync())
            .ReturnsAsync(ResultOfT<IReadOnlyList<WorkoutTypeResponse>>.Success(response));

        var result = await _controller.GetAll();

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(response));
    }

    [Test]
    public async Task GetAll_WhenServiceFailsWithUnexpectedError_ReturnsBadRequestObjectResult()
    {
        _workoutTypeServiceMock
            .Setup(service => service.GetAllWorkoutTypesAsync())
            .ReturnsAsync(ResultOfT<IReadOnlyList<WorkoutTypeResponse>>.Failure(new Error(ErrorCode.UnexpectedError, "Failed")));

        var result = await _controller.GetAll();

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }
}
