using Moq;
using NotificationService.Application.Services.v1;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;
using Shared.Abstractions.Auth;
using Shared.Kernal.Results;

namespace NotificationService.Application.Tests.Services.v1;

[TestFixture]
public class RemindersServiceTests
{
    private Mock<IRemindersRepository> _repositoryMock = null!;
    private Mock<ICurrentUser> _currentUserMock = null!;
    private RemindersService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IRemindersRepository>(MockBehavior.Strict);
        _currentUserMock = new Mock<ICurrentUser>(MockBehavior.Strict);
        _service = new RemindersService(_repositoryMock.Object, _currentUserMock.Object);
    }

    [Test]
    public void GetAllRemindersAsync_WhenNoGoalReminders_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(u => u.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Success(userId));

        _repositoryMock.Setup(r => r.GetGoalReminders(userId))
            .Returns([]);

        var result = _service.GetAllRemindersAsync();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.Empty);
    }

    [Test]
    public void GetAllRemindersAsync_WhenGoalRemindersExist_ReturnsFormattedMessages()
    {
        var userId = Guid.NewGuid();
        var goalId = Guid.NewGuid();

        _currentUserMock.Setup(u => u.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Success(userId));

        _repositoryMock.Setup(r => r.GetGoalReminders(userId))
            .Returns([
                new GoalReminder
                {
                    Id = goalId,
                    GoalId = goalId,
                    Name = "Get Fit",
                    MetricType = 0,
                    PeriodType = 1,
                    TargetValue = 20,
                    CurrentValue = 15,
                    Remaining = 5
                }
            ]);

        var result = _service.GetAllRemindersAsync();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Has.Count.EqualTo(1));
        Assert.That(result.Value[0].Message, Does.Contain("5"));
        Assert.That(result.Value[0].Message, Does.Contain("workouts"));
        Assert.That(result.Value[0].Message, Does.Contain("Get Fit"));
        Assert.That(result.Value[0].Message, Does.Contain("this month"));
    }

    [Test]
    [TestCase(0, "workouts")]
    [TestCase(1, "minutes")]
    [TestCase(2, "km")]
    [TestCase(3, "calories")]
    [TestCase(4, "days")]
    [TestCase(5, "workout types")]
    [TestCase(6, "workouts")]
    [TestCase(7, "workouts")]
    public void GetAllRemindersAsync_MetricTypeUnit_IsCorrectlyResolved(int metricType, string expectedUnit)
    {
        var userId = Guid.NewGuid();
        var goalId = Guid.NewGuid();

        _currentUserMock.Setup(u => u.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Success(userId));

        _repositoryMock.Setup(r => r.GetGoalReminders(userId))
            .Returns([
                new GoalReminder
                {
                    Id = goalId,
                    GoalId = goalId,
                    Name = "Test Goal",
                    MetricType = metricType,
                    PeriodType = 0,
                    TargetValue = 10,
                    CurrentValue = 5,
                    Remaining = 5
                }
            ]);

        var result = _service.GetAllRemindersAsync();

        Assert.That(result.Value[0].Message, Does.Contain(expectedUnit));
    }

    [Test]
    [TestCase(0, "this week")]
    [TestCase(1, "this month")]
    [TestCase(3, "in the next 7 days")]
    public void GetAllRemindersAsync_PeriodTypeDescription_IsCorrectlyResolved(int periodType, string expectedPeriod)
    {
        var userId = Guid.NewGuid();
        var goalId = Guid.NewGuid();

        _currentUserMock.Setup(u => u.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Success(userId));

        _repositoryMock.Setup(r => r.GetGoalReminders(userId))
            .Returns([
                new GoalReminder
                {
                    Id = goalId,
                    GoalId = goalId,
                    Name = "Test Goal",
                    MetricType = 0,
                    PeriodType = periodType,
                    TargetValue = 10,
                    CurrentValue = 5,
                    Remaining = 5
                }
            ]);

        var result = _service.GetAllRemindersAsync();

        Assert.That(result.Value[0].Message, Does.Contain(expectedPeriod));
    }

    [Test]
    public void GetAllRemindersAsync_WhenCustomRangeWithEndDate_IncludesEndDateInMessage()
    {
        var userId = Guid.NewGuid();
        var goalId = Guid.NewGuid();
        var endDate = new DateTime(2026, 5, 15);

        _currentUserMock.Setup(u => u.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Success(userId));

        _repositoryMock.Setup(r => r.GetGoalReminders(userId))
            .Returns([
                new GoalReminder
                {
                    Id = goalId,
                    GoalId = goalId,
                    Name = "Run 10k",
                    MetricType = 2,
                    PeriodType = 2,
                    TargetValue = 100,
                    CurrentValue = 80,
                    Remaining = 20,
                    EndDate = endDate
                }
            ]);

        var result = _service.GetAllRemindersAsync();

        Assert.That(result.Value[0].Message, Does.Contain("by"));
        Assert.That(result.Value[0].Message, Does.Contain("2026"));
    }

    [Test]
    public void GetAllRemindersAsync_WhenCustomRangeWithoutEndDate_UsesDefaultDescription()
    {
        var userId = Guid.NewGuid();
        var goalId = Guid.NewGuid();

        _currentUserMock.Setup(u => u.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Success(userId));

        _repositoryMock.Setup(r => r.GetGoalReminders(userId))
            .Returns([
                new GoalReminder
                {
                    Id = goalId,
                    GoalId = goalId,
                    Name = "Open Goal",
                    MetricType = 0,
                    PeriodType = 2,
                    TargetValue = 10,
                    CurrentValue = 5,
                    Remaining = 5,
                    EndDate = null
                }
            ]);

        var result = _service.GetAllRemindersAsync();

        Assert.That(result.Value[0].Message, Does.Contain("in the custom period"));
    }
}
