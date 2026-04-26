using System.Net;
using System.Net.Http.Json;
using AnalyticsService.Api.Integration.Infrastructure;
using AnalyticsService.Application.Dtos.v1.Responses;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace AnalyticsService.Api.Integration.Controllers.v1;

[TestFixture]
[NonParallelizable]
public class ProfileAnalyticsControllerIntegrationTests
{
    private AnalyticsApiFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new AnalyticsApiFactory();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        _factory.ProfileAnalyticsService.Reset();
        _client = _factory.CreateAuthenticatedClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    [Test]
    public async Task GetProfileAnalytics_WhenServiceReturnsSuccess_Returns200WithPayload()
    {
        // Arrange
        _factory.ProfileAnalyticsService.ProfileHandler = _ =>
            Task.FromResult(ResultOfT<ProfileAnalyticsResponse>.Success(new ProfileAnalyticsResponse
            {
                TotalWorkoutsCompleted = 44,
                TotalHoursTrained = 25,
                GoalsAchieved = 5,
                WorkoutsThisWeek = 3
            }));

        // Act
        var response = await _client.GetAsync("/api/v1/profile-analytics");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var payload = await response.Content.ReadFromJsonAsync<ProfileAnalyticsResponse>();
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload!.GoalsAchieved, Is.EqualTo(5));
        Assert.That(payload.TotalWorkoutsCompleted, Is.EqualTo(44));
    }

    [Test]
    public async Task GetProfileAnalytics_WhenServiceReturnsValidationError_Returns400()
    {
        // Arrange
        _factory.ProfileAnalyticsService.ProfileHandler = _ =>
            Task.FromResult(ResultOfT<ProfileAnalyticsResponse>.Failure(new Error(ErrorCode.ValidationFailed,
                "Invalid profile analytics request.")));

        // Act
        var response = await _client.GetAsync("/api/v1/profile-analytics");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var payload = await response.Content.ReadAsStringAsync();
        Assert.That(payload, Does.Contain("Invalid profile analytics request."));
    }

    [Test]
    public async Task GetProfileAnalytics_WhenNoAuthHeader_Returns401()
    {
        // Arrange
        using var anonymousClient = _factory.CreateClient();

        // Act
        var response = await anonymousClient.GetAsync("/api/v1/profile-analytics");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
