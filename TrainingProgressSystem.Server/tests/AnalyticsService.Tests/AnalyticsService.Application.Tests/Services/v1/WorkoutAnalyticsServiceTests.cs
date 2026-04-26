using AnalyticsService.Application.Dtos.v1.Responses;
using AnalyticsService.Application.Interfaces.v1;
using AnalyticsService.Application.Services.v1;
using AnalyticsService.Domain.Interfaces.v1;
using AnalyticsService.Domain.Models;
using Moq;
using Shared.Abstractions.Auth;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace AnalyticsService.Application.Tests.Services.v1;

[TestFixture]
public class WorkoutAnalyticsServiceTests
{
    private Mock<IWorkoutRepository> _workoutRepository = null!;
    private Mock<IAnalyticsSnapshotService> _snapshotService = null!;
    private Mock<ICurrentUser> _currentUser = null!;
    private WorkoutAnalyticsService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _workoutRepository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        _snapshotService = new Mock<IAnalyticsSnapshotService>(MockBehavior.Strict);
        _currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        _service = new WorkoutAnalyticsService(
            _workoutRepository.Object,
            _snapshotService.Object,
            _currentUser.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<WorkoutAnalyticsService>>());
    }

    [Test]
    public async Task GetSummaryAsync_WhenCurrentUserFails_ReturnsFailure()
    {
        _currentUser.Setup(user => user.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Failure(new Error(ErrorCode.Unauthorized, "Unauthorized")));

        var result = await _service.GetSummaryAsync();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.Unauthorized));
    }

    [Test]
    public async Task GetSummaryAsync_WhenSnapshotAvailable_ReturnsSummaryFromSnapshot()
    {
        var userId = Guid.NewGuid();

        _currentUser.Setup(user => user.GetCurrentUserId()).Returns(ResultOfT<Guid>.Success(userId));
        _snapshotService.Setup(service => service.GetSnapshotAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<AnalyticsSnapshotData>.Success(new AnalyticsSnapshotData
            {
                Summary = new WorkoutSummaryResponse
                {
                    AmountPerWeek = 3,
                    WeekDurationMin = 100,
                    AmountThisMonth = 12,
                    MonthlyTimeMin = 420
                }
            }));

        var result = await _service.GetSummaryAsync();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.AmountPerWeek, Is.EqualTo(3));
        Assert.That(result.Value.MonthlyTimeMin, Is.EqualTo(420));
    }

    [Test]
    public async Task GetDailyTrendAsync_WhenDaysSeven_ReturnsSnapshotData()
    {
        var userId = Guid.NewGuid();

        _currentUser.Setup(user => user.GetCurrentUserId()).Returns(ResultOfT<Guid>.Success(userId));
        _snapshotService.Setup(service => service.GetSnapshotAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<AnalyticsSnapshotData>.Success(new AnalyticsSnapshotData
            {
                DailyTrendLast7Days =
                [
                    new WorkoutDailyTrendPointResponse { Date = new DateTime(2026, 4, 20), WorkoutsCount = 2, DurationMin = 60 }
                ]
            }));

        var result = await _service.GetDailyTrendAsync(7);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.Count, Is.EqualTo(1));
        Assert.That(result.Value[0].WorkoutsCount, Is.EqualTo(2));
        _workoutRepository.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GetDailyTrendAsync_WhenDaysNotSeven_UsesRepository()
    {
        var userId = Guid.NewGuid();

        _currentUser.Setup(user => user.GetCurrentUserId()).Returns(ResultOfT<Guid>.Success(userId));
        _workoutRepository.Setup(repository => repository.GetDailyTrendByPeriodAsync(
                userId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<IReadOnlyList<DailyWorkoutTrendPoint>>.Success([
                new DailyWorkoutTrendPoint { Date = new DateTime(2026, 4, 20), WorkoutsCount = 1, DurationMin = 40 }
            ]));

        var result = await _service.GetDailyTrendAsync(10);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.Count, Is.EqualTo(1));
        _workoutRepository.Verify(repository => repository.GetDailyTrendByPeriodAsync(
            userId,
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetCountByTypeAsync_WhenNoRangeProvided_ReturnsSnapshotData()
    {
        var userId = Guid.NewGuid();

        _currentUser.Setup(user => user.GetCurrentUserId()).Returns(ResultOfT<Guid>.Success(userId));
        _snapshotService.Setup(service => service.GetSnapshotAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<AnalyticsSnapshotData>.Success(new AnalyticsSnapshotData
            {
                CountByTypeLast7Days =
                [
                    new WorkoutCountByTypeResponse
                    {
                        WorkoutTypeId = Guid.NewGuid(),
                        WorkoutTypeName = "Cardio",
                        WorkoutsCount = 4
                    }
                ]
            }));

        var result = await _service.GetCountByTypeAsync();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.Count, Is.EqualTo(1));
        Assert.That(result.Value[0].WorkoutTypeName, Is.EqualTo("Cardio"));
    }

    [Test]
    public async Task GetCountByTypeAsync_WhenRangeProvided_UsesRepository()
    {
        var userId = Guid.NewGuid();
        var from = new DateTime(2026, 4, 1);
        var to = new DateTime(2026, 4, 21);

        _currentUser.Setup(user => user.GetCurrentUserId()).Returns(ResultOfT<Guid>.Success(userId));
        _workoutRepository.Setup(repository => repository.GetCountByTypeAsync(
                userId,
                from,
                to.AddDays(1),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<IReadOnlyList<WorkoutCountByType>>.Success([
                new WorkoutCountByType { WorkoutTypeId = Guid.NewGuid(), WorkoutTypeName = "Strength", WorkoutsCount = 6 }
            ]));

        var result = await _service.GetCountByTypeAsync(from, to);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value[0].WorkoutTypeName, Is.EqualTo("Strength"));
    }

    [Test]
    public async Task GetStatisticsOverviewAsync_WhenSnapshotAvailable_ReturnsStatisticsFromSnapshot()
    {
        var userId = Guid.NewGuid();

        _currentUser.Setup(user => user.GetCurrentUserId()).Returns(ResultOfT<Guid>.Success(userId));
        _snapshotService.Setup(service => service.GetSnapshotAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultOfT<AnalyticsSnapshotData>.Success(new AnalyticsSnapshotData
            {
                StatisticsOverview = new WorkoutStatisticsOverviewResponse
                {
                    TotalAchievedGoals = 9,
                    TotalTrainingHours = 2.5,
                    TotalWorkoutsCompleted = 30,
                    WorkoutsThisWeek = 5
                }
            }));

        var result = await _service.GetStatisticsOverviewAsync();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.TotalAchievedGoals, Is.EqualTo(9));
        Assert.That(result.Value.TotalWorkoutsCompleted, Is.EqualTo(30));
    }
}
