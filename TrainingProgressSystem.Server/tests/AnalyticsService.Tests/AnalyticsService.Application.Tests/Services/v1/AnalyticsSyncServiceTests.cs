using AnalyticsService.Application.Dtos.v1.Responses;
using AnalyticsService.Application.Interfaces.v1;
using AnalyticsService.Application.Services.v1;
using Moq;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace AnalyticsService.Application.Tests.Services.v1;

[TestFixture]
public class AnalyticsSyncServiceTests
{
    private Mock<IAnalyticsSnapshotService> _snapshotService = null!;
    private AnalyticsSyncService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _snapshotService = new Mock<IAnalyticsSnapshotService>(MockBehavior.Strict);
        _service = new AnalyticsSyncService(
            _snapshotService.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<AnalyticsSyncService>>());
    }

    [Test]
    public async Task RecalculateForWorkoutAsync_WhenRefreshFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();

        _snapshotService
            .Setup(service => service.RefreshSnapshotAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<AnalyticsSnapshotData>.Failure(new Error(ErrorCode.UnexpectedError, "failed")));

        var result = await _service.RecalculateForWorkoutAsync(userId, workoutId);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.UnexpectedError));
    }

    [Test]
    public async Task RecalculateForWorkoutAsync_WhenRefreshSucceeds_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();

        _snapshotService
            .Setup(service => service.RefreshSnapshotAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<AnalyticsSnapshotData>.Success(new AnalyticsSnapshotData
            {
                LastCalculatedAtUtc = DateTime.UtcNow
            }));

        var result = await _service.RecalculateForWorkoutAsync(userId, workoutId);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.True);
        _snapshotService.Verify(service => service.RefreshSnapshotAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
