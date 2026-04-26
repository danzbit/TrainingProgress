using AnalyticsService.Application.Dtos.v1.Responses;
using AnalyticsService.Application.Interfaces.v1;
using AnalyticsService.Application.Services.v1;
using Moq;
using Shared.Abstractions.Auth;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace AnalyticsService.Application.Tests.Services.v1;

[TestFixture]
public class ProfileAnalyticsServiceTests
{
    private Mock<IAnalyticsSnapshotService> _snapshotService = null!;
    private Mock<ICurrentUser> _currentUser = null!;
    private ProfileAnalyticsService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _snapshotService = new Mock<IAnalyticsSnapshotService>(MockBehavior.Strict);
        _currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        _service = new ProfileAnalyticsService(
            _snapshotService.Object,
            _currentUser.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ProfileAnalyticsService>>());
    }

    [Test]
    public async Task GetProfileAnalyticsAsync_WhenCurrentUserFails_ReturnsFailure()
    {
        _currentUser
            .Setup(user => user.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Failure(new Error(ErrorCode.Unauthorized, "Unauthorized")));

        var result = await _service.GetProfileAnalyticsAsync();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.Unauthorized));
        _snapshotService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GetProfileAnalyticsAsync_WhenSnapshotFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();

        _currentUser.Setup(user => user.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Success(userId));

        _snapshotService.Setup(service => service.GetSnapshotAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<AnalyticsSnapshotData>.Failure(new Error(ErrorCode.UnexpectedError, "failed")));

        var result = await _service.GetProfileAnalyticsAsync();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.UnexpectedError));
    }

    [Test]
    public async Task GetProfileAnalyticsAsync_WhenSnapshotSucceeds_ReturnsProfileAnalytics()
    {
        var userId = Guid.NewGuid();

        _currentUser.Setup(user => user.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Success(userId));

        _snapshotService.Setup(service => service.GetSnapshotAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<AnalyticsSnapshotData>.Success(new AnalyticsSnapshotData
            {
                ProfileAnalytics = new ProfileAnalyticsResponse
                {
                    TotalWorkoutsCompleted = 42,
                    TotalHoursTrained = 2.08,
                    GoalsAchieved = 6,
                    WorkoutsThisWeek = 4
                }
            }));

        var result = await _service.GetProfileAnalyticsAsync();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.TotalWorkoutsCompleted, Is.EqualTo(42));
        Assert.That(result.Value.TotalHoursTrained, Is.EqualTo(2.08));
        Assert.That(result.Value.GoalsAchieved, Is.EqualTo(6));
        Assert.That(result.Value.WorkoutsThisWeek, Is.EqualTo(4));
    }
}
