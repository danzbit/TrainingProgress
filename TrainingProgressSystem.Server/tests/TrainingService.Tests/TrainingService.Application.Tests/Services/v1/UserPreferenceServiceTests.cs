using Moq;
using Shared.Abstractions.Auth;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Services.v1;
using TrainingService.Domain.Entities;
using TrainingService.Domain.Interfaces.v1;

namespace TrainingService.Application.Tests.Services.v1;

[TestFixture]
public class UserPreferenceServiceTests
{
    [Test]
    public async Task GetPreferenceAsync_WhenRepositoryReturnsNull_ReturnsDefaultListMode()
    {
        var userId = Guid.NewGuid();
        var repositoryMock = new Mock<IUserPreferenceRepository>();
        var currentUserMock = new Mock<ICurrentUser>();

        currentUserMock
            .Setup(x => x.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Success(userId));

        repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<UserPreference?>.Success(null));

        var service = new UserPreferenceService(repositoryMock.Object, currentUserMock.Object);

        var result = await service.GetPreferenceAsync();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.HistoryViewMode, Is.EqualTo("list"));
    }

    [Test]
    public async Task UpdatePreferenceAsync_WhenInvalidViewMode_ReturnsValidationFailure()
    {
        var repositoryMock = new Mock<IUserPreferenceRepository>();
        var currentUserMock = new Mock<ICurrentUser>();

        currentUserMock
            .Setup(x => x.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Success(Guid.NewGuid()));

        var service = new UserPreferenceService(repositoryMock.Object, currentUserMock.Object);

        var result = await service.UpdatePreferenceAsync(new UpdateUserPreferenceRequest("grid"));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.ValidationFailed));
        repositoryMock.Verify(
            x => x.UpsertAsync(It.IsAny<UserPreference>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
