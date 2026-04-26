using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Api.Controllers.v1;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Application.Interfaces.v1;

namespace TrainingService.Api.Tests.Controllers.v1;

[TestFixture]
public class AchievementsControllerTests
{
    private Mock<IAchievementService> _achievementServiceMock = null!;
    private AchievementsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _achievementServiceMock = new Mock<IAchievementService>(MockBehavior.Strict);
        _controller = new AchievementsController(_achievementServiceMock.Object);
    }

    [Test]
    public async Task ShareProgress_WhenServiceSucceeds_ReturnsOkObjectResult()
    {
        var response = new ShareProgressResponse("public-key");

        _achievementServiceMock
            .Setup(service => service.ShareProgressAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<ShareProgressResponse>.Success(response));

        var result = await _controller.ShareProgress(CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(response));
        _achievementServiceMock.Verify(service => service.ShareProgressAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetSharedProgress_WhenServiceFailsWithEntityNotFound_ReturnsNotFoundObjectResult()
    {
        _achievementServiceMock
            .Setup(service => service.GetSharedProgressAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<SharedProgressResponse>.Failure(new Error(ErrorCode.EntityNotFound, "Not found")));

        var result = await _controller.GetSharedProgress("missing", CancellationToken.None);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        _achievementServiceMock.Verify(service => service.GetSharedProgressAsync("missing", It.IsAny<CancellationToken>()), Times.Once);
    }
}
