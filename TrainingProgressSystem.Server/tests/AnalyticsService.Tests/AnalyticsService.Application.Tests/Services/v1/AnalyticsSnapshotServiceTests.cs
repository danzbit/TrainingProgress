using AnalyticsService.Application.Dtos.v1.Responses;
using AnalyticsService.Application.Services.v1;
using AnalyticsService.Domain.Entities;
using AnalyticsService.Domain.Interfaces.v1;
using AnalyticsService.Domain.Models;
using Moq;
using Shared.Abstractions.Caching;
using Shared.Kernal.Results;

namespace AnalyticsService.Application.Tests.Services.v1;

[TestFixture]
public class AnalyticsSnapshotServiceTests
{
    private Mock<IWorkoutRepository> _workoutRepository = null!;
    private Mock<IAnalyticsSnapshotRepository> _snapshotRepository = null!;
    private Mock<ICacheService> _cacheService = null!;
    private AnalyticsSnapshotService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _workoutRepository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        _snapshotRepository = new Mock<IAnalyticsSnapshotRepository>(MockBehavior.Strict);
        _cacheService = new Mock<ICacheService>(MockBehavior.Strict);

        _service = new AnalyticsSnapshotService(
            _workoutRepository.Object,
            _snapshotRepository.Object,
            _cacheService.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<AnalyticsSnapshotService>>());
    }

    [Test]
    public async Task GetSnapshotAsync_WhenFoundInCache_ReturnsCachedSnapshot()
    {
        var userId = Guid.NewGuid();
        var cached = new AnalyticsSnapshotData
        {
            Summary = new WorkoutSummaryResponse { AmountPerWeek = 10 },
            LastCalculatedAtUtc = DateTime.UtcNow
        };

        _cacheService
            .Setup(cache => cache.GetAsync<AnalyticsSnapshotData>($"analytics:snapshot:{userId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var result = await _service.GetSnapshotAsync(userId);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.Summary.AmountPerWeek, Is.EqualTo(10));
        _snapshotRepository.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GetSnapshotAsync_WhenCacheMissAndDbHit_ReturnsMappedSnapshotAndCachesIt()
    {
        var userId = Guid.NewGuid();

        _cacheService
            .Setup(cache => cache.GetAsync<AnalyticsSnapshotData>($"analytics:snapshot:{userId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AnalyticsSnapshotData?)null);

        _snapshotRepository
            .Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<AnalyticsSnapshot?>.Success(new AnalyticsSnapshot
            {
                UserId = userId,
                AmountPerWeek = 3,
                WeekDurationMin = 100,
                AmountThisMonth = 15,
                MonthlyTimeMin = 450,
                TotalAchievedGoals = 5,
                TotalWorkoutsCompleted = 35,
                WorkoutsThisWeek = 3,
                TotalTrainingHours = 12.5,
                DailyTrendJson = "[]",
                CountByTypeJson = "[]",
                LastCalculatedAtUtc = DateTime.UtcNow
            }));

        _cacheService
            .Setup(cache => cache.SetAsync(
                $"analytics:snapshot:{userId}",
                It.IsAny<AnalyticsSnapshotData>(),
                null,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.GetSnapshotAsync(userId);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.Summary.AmountPerWeek, Is.EqualTo(3));
        Assert.That(result.Value.ProfileAnalytics.TotalWorkoutsCompleted, Is.EqualTo(35));
    }

    [Test]
    public async Task RefreshSnapshotAsync_WhenSuccessful_PersistsAndCachesSnapshot()
    {
        var userId = Guid.NewGuid();

        _workoutRepository
            .Setup(repository => repository.GetAggregateByPeriodAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ResultOfT<WorkoutAggregate>.Success(new WorkoutAggregate { WorkoutsCount = 4, DurationMin = 120 }));

        _workoutRepository
            .Setup(repository => repository.GetDailyTrendByPeriodAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<IReadOnlyList<DailyWorkoutTrendPoint>>.Success([
                new DailyWorkoutTrendPoint { Date = DateTime.UtcNow.Date, WorkoutsCount = 1, DurationMin = 30 }
            ]));

        _workoutRepository
            .Setup(repository => repository.GetCountByTypeAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<IReadOnlyList<WorkoutCountByType>>.Success([
                new WorkoutCountByType { WorkoutTypeId = Guid.NewGuid(), WorkoutTypeName = "Cardio", WorkoutsCount = 2 }
            ]));

        _workoutRepository
            .Setup(repository => repository.GetStatisticsOverviewAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<WorkoutStatisticsOverview>.Success(new WorkoutStatisticsOverview
            {
                TotalAchievedGoals = 6,
                TotalTrainingMinutes = 240,
                TotalWorkoutsCompleted = 40,
                WorkoutsThisWeek = 4
            }));

        _workoutRepository
            .Setup(repository => repository.GetTotalWorkoutsCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<int>.Success(40));

        _workoutRepository
            .Setup(repository => repository.GetTotalDurationMinutesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<int>.Success(600));

        _workoutRepository
            .Setup(repository => repository.GetAchievedGoalsCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<int>.Success(6));

        _snapshotRepository
            .Setup(repository => repository.UpsertAsync(It.IsAny<AnalyticsSnapshot>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _cacheService
            .Setup(cache => cache.SetAsync(
                $"analytics:snapshot:{userId}",
                It.IsAny<AnalyticsSnapshotData>(),
                null,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.RefreshSnapshotAsync(userId);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.Summary.AmountPerWeek, Is.EqualTo(4));
        Assert.That(result.Value.StatisticsOverview.TotalWorkoutsCompleted, Is.EqualTo(40));
        _snapshotRepository.Verify(repository => repository.UpsertAsync(It.IsAny<AnalyticsSnapshot>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
