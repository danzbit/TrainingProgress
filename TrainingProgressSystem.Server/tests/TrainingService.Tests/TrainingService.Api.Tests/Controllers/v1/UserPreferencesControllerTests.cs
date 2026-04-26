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
public class UserPreferencesControllerTests
{
    private Mock<IUserPreferenceService> _preferencesServiceMock = null!;
    private UserPreferencesController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _preferencesServiceMock = new Mock<IUserPreferenceService>(MockBehavior.Strict);
        _controller = new UserPreferencesController(_preferencesServiceMock.Object);
    }

    [Test]
    public async Task Get_WhenServiceSucceeds_ReturnsOkObjectResult()
    {
        var response = new UserPreferenceResponse("week");

        _preferencesServiceMock
            .Setup(service => service.GetPreferenceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<UserPreferenceResponse>.Success(response));

        var result = await _controller.Get(CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(response));
    }

    [Test]
    public async Task Update_WhenServiceFailsWithValidationError_ReturnsBadRequestObjectResult()
    {
        var request = new UpdateUserPreferenceRequest("day");

        _preferencesServiceMock
            .Setup(service => service.UpdatePreferenceAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error(ErrorCode.ValidationFailed, "Invalid")));

        var result = await _controller.Update(request, CancellationToken.None);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }
}
