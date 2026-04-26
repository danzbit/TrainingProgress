using Microsoft.AspNetCore.Mvc;
using Moq;
using NotificationService.Api.Controllers.v1;
using NotificationService.Application.Dtos.v1.Responses;
using NotificationService.Application.Interfaces.v1;
using Shared.Kernal.Results;

namespace NotificationService.Api.Tests.Controllers.v1;

[TestFixture]
public class RemindersControllerTests
{
    private Mock<IRemindersService> _remindersServiceMock = null!;
    private RemindersController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _remindersServiceMock = new Mock<IRemindersService>(MockBehavior.Strict);
        _controller = new RemindersController(_remindersServiceMock.Object);
    }

    [Test]
    public void GetAll_WhenServiceReturnsReminders_ReturnsOkWithReminders()
    {
        // Arrange
        var reminders = new List<ReminderResponse>
        {
            new("📌 You need 5 more workouts to reach your \"Get Fit\" goal this month."),
            new("📌 You need 100 more minutes to reach your \"Run 10k\" goal by 2026-05-15."),
            new("📌 You need 20 more km to reach your \"Marathon Training\" goal in the next 7 days.")
        };

        var successResult = ResultOfT<IReadOnlyList<ReminderResponse>>.Success(reminders.AsReadOnly());

        _remindersServiceMock
            .Setup(s => s.GetAllRemindersAsync())
            .Returns(successResult);

        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        var returnedReminders = okResult.Value as IReadOnlyList<ReminderResponse>;
        Assert.That(returnedReminders, Is.Not.Null);
        Assert.That(returnedReminders!.Count, Is.EqualTo(3));
        Assert.That(returnedReminders[0].Message, Contains.Substring("Get Fit"));
        Assert.That(returnedReminders[1].Message, Contains.Substring("Run 10k"));
        Assert.That(returnedReminders[2].Message, Contains.Substring("Marathon Training"));

        _remindersServiceMock.Verify(s => s.GetAllRemindersAsync(), Times.Once);
    }

    [Test]
    public void GetAll_WhenServiceReturnsEmptyList_ReturnsOkWithEmptyReminders()
    {
        // Arrange
        var successResult = ResultOfT<IReadOnlyList<ReminderResponse>>.Success(
            new List<ReminderResponse>().AsReadOnly());

        _remindersServiceMock
            .Setup(s => s.GetAllRemindersAsync())
            .Returns(successResult);

        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        var returnedReminders = okResult.Value as IReadOnlyList<ReminderResponse>;
        Assert.That(returnedReminders, Is.Not.Null);
        Assert.That(returnedReminders!.Count, Is.EqualTo(0));

        _remindersServiceMock.Verify(s => s.GetAllRemindersAsync(), Times.Once);
    }
}
