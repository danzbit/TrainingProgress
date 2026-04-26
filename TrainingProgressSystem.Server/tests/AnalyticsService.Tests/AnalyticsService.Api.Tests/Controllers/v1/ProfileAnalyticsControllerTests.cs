using AnalyticsService.Api.Controllers.v1;
using AnalyticsService.Application.Dtos.v1.Responses;
using AnalyticsService.Application.Interfaces.v1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace AnalyticsService.Api.Tests.Controllers.v1;

[TestFixture]
public class ProfileAnalyticsControllerTests
{
    private Mock<IProfileAnalyticsService> _serviceMock = null!;
    private ProfileAnalyticsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IProfileAnalyticsService>(MockBehavior.Strict);
        var logger = Mock.Of<ILogger<ProfileAnalyticsController>>();
        _controller = new ProfileAnalyticsController(_serviceMock.Object, logger);
    }

    [Test]
    public async Task GetProfileAnalytics_WhenServiceSucceeds_ReturnsOkObjectResult()
    {
        var expected = new ProfileAnalyticsResponse { GoalsAchieved = 5, TotalWorkoutsCompleted = 44 };

        _serviceMock
            .Setup(s => s.GetProfileAnalyticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<ProfileAnalyticsResponse>.Success(expected));

        var result = await _controller.GetProfileAnalytics(CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(expected));
        _serviceMock.Verify(s => s.GetProfileAnalyticsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetProfileAnalytics_WhenServiceReturnsValidationError_ReturnsBadRequestObjectResult()
    {
        _serviceMock
            .Setup(s => s.GetProfileAnalyticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<ProfileAnalyticsResponse>.Failure(
                new Error(ErrorCode.ValidationFailed, "Validation failed")));

        var result = await _controller.GetProfileAnalytics(CancellationToken.None);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }
}
