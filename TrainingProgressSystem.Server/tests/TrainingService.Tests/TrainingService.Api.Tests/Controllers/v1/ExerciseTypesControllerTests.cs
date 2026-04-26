using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Api.Controllers.v1;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;

namespace TrainingService.Api.Tests.Controllers.v1;

[TestFixture]
public class ExerciseTypesControllerTests
{
    private Mock<IExerciseTypeService> _exerciseTypeServiceMock = null!;
    private ExerciseTypesController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _exerciseTypeServiceMock = new Mock<IExerciseTypeService>(MockBehavior.Strict);
        _controller = new ExerciseTypesController(_exerciseTypeServiceMock.Object);
    }

    [Test]
    public async Task GetAll_WhenServiceSucceeds_ReturnsOkObjectResult()
    {
        IReadOnlyList<ExerciseTypeResponse> response =
        [
            new ExerciseTypeResponse(Guid.NewGuid(), "Bench Press", "Strength")
        ];

        _exerciseTypeServiceMock
            .Setup(service => service.GetAllExerciseTypesAsync())
            .ReturnsAsync(ResultOfT<IReadOnlyList<ExerciseTypeResponse>>.Success(response));

        var result = await _controller.GetAll();

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(response));
    }

    [Test]
    public async Task GetAll_WhenServiceFails_ReturnsBadRequestObjectResult()
    {
        _exerciseTypeServiceMock
            .Setup(service => service.GetAllExerciseTypesAsync())
            .ReturnsAsync(ResultOfT<IReadOnlyList<ExerciseTypeResponse>>.Failure(new Error(ErrorCode.ValidationFailed, "Invalid")));

        var result = await _controller.GetAll();

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }
}
