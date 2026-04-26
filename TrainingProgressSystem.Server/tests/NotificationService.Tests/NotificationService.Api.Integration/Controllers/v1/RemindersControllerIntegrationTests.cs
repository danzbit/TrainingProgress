using System.Net;
using System.Net.Http.Json;
using NotificationService.Api.Integration.Infrastructure;
using NotificationService.Application.Dtos.v1.Responses;
using Shared.Kernal.Errors;

namespace NotificationService.Api.Integration.Controllers.v1;

[TestFixture]
[NonParallelizable]
public class RemindersControllerIntegrationTests
{
    private NotificationServiceApiFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new NotificationServiceApiFactory();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _client?.Dispose();
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }
    }

    [SetUp]
    public void SetUp()
    {
        _factory.RemindersService.Reset();
        _client = _factory.CreateAuthenticatedClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
    }

    [Test]
    public async Task GetAll_WithValidAuthentication_Returns200AndReminders()
    {
        // Arrange
        var reminders = new List<ReminderResponse>
        {
            new("Reminder 1"),
            new("Reminder 2"),
            new("Reminder 3")
        };
        _factory.RemindersService.ReminderResponses = reminders;

        // Act
        var response = await _client.GetAsync("/api/v1/reminders");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_factory.RemindersService.GetAllRemindersCallCount, Is.EqualTo(1));

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("Reminder 1"));
        Assert.That(content, Does.Contain("Reminder 2"));
        Assert.That(content, Does.Contain("Reminder 3"));
    }

    [Test]
    public async Task GetAll_WhenNoReminders_Returns200WithEmptyList()
    {
        // Arrange
        _factory.RemindersService.ReminderResponses = new();

        // Act
        var response = await _client.GetAsync("/api/v1/reminders");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_factory.RemindersService.GetAllRemindersCallCount, Is.EqualTo(1));

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("[]"));
    }

    [Test]
    public async Task GetAll_WhenServiceReturnsError_Returns400WithErrorDetails()
    {
        // Arrange
        var reminders = new List<ReminderResponse> { new("Reminder 1") };
        _factory.RemindersService.ReminderResponses = reminders;
        _factory.RemindersService.ExpectedError = new Error(
            ErrorCode.UnexpectedError,
            "Failed to retrieve reminders");

        // Act
        var response = await _client.GetAsync("/api/v1/reminders");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(_factory.RemindersService.GetAllRemindersCallCount, Is.EqualTo(1));

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("Failed to retrieve reminders"));
    }

    [Test]
    public async Task GetAll_WhenAnonymous_Returns401()
    {
        // Arrange
        using var anonymousClient = _factory.CreateClient();

        // Act
        var response = await anonymousClient.GetAsync("/api/v1/reminders");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetAll_MultipleRequests_CallsServiceMultipleTimes()
    {
        // Arrange
        _factory.RemindersService.ReminderResponses = new() { new("Reminder 1") };

        // Act
        await _client.GetAsync("/api/v1/reminders");
        await _client.GetAsync("/api/v1/reminders");
        await _client.GetAsync("/api/v1/reminders");

        // Assert
        Assert.That(_factory.RemindersService.GetAllRemindersCallCount, Is.EqualTo(3));
    }
}
