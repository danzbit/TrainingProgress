using Microsoft.Extensions.Logging;
using Moq;
using Shared.Abstractions.Auth;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Application.Services.v1;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Interfaces.v1;

namespace TrainingService.Application.Tests.Services.v1;

[TestFixture]
public class AchievementServiceTests
{
    [Test]
    public async Task ShareProgressAsync_WhenCurrentUserInvalid_ReturnsFailure()
    {
        var repositoryMock = new Mock<IAchievementRepository>();
        var currentUserMock = new Mock<ICurrentUser>();
        var loggerMock = new Mock<ILogger<AchievementService>>();

        var error = new Error(ErrorCode.Unauthorized, "Invalid user");
        currentUserMock
            .Setup(x => x.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Failure(error));

        var service = new AchievementService(repositoryMock.Object, currentUserMock.Object, loggerMock.Object);

        var result = await service.ShareProgressAsync();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(error));
    }

    [Test]
    public async Task ShareProgressAsync_WhenExistingLinkExists_ReturnsExistingLink()
    {
        var userId = Guid.NewGuid();
        var repositoryMock = new Mock<IAchievementRepository>();
        var currentUserMock = new Mock<ICurrentUser>();
        var loggerMock = new Mock<ILogger<AchievementService>>();

        currentUserMock
            .Setup(x => x.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Success(userId));

        repositoryMock
            .Setup(x => x.GetActiveSharedAchievementByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<SharedAchievement?>.Success(new SharedAchievement
            {
                PublicUrlKey = "existing-key"
            }));

        var service = new AchievementService(repositoryMock.Object, currentUserMock.Object, loggerMock.Object);

        var result = await service.ShareProgressAsync();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.PublicUrlKey, Is.EqualTo("existing-key"));
        repositoryMock.Verify(
            x => x.AddAchievementAsync(It.IsAny<Achievement>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetSharedProgressAsync_WhenNotFound_ReturnsEntityNotFound()
    {
        var repositoryMock = new Mock<IAchievementRepository>();
        var currentUserMock = new Mock<ICurrentUser>();
        var loggerMock = new Mock<ILogger<AchievementService>>();

        repositoryMock
            .Setup(x => x.GetActiveSharedAchievementByKeyAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<SharedAchievement?>.Success(null));

        var service = new AchievementService(repositoryMock.Object, currentUserMock.Object, loggerMock.Object);

        var result = await service.GetSharedProgressAsync("missing");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(Error.EntityNotFound));
    }
}
