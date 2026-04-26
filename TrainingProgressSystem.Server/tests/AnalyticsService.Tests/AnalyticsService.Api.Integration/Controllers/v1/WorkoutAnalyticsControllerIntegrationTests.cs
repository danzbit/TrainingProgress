using System.Net;
using System.Net.Http.Json;
using AnalyticsService.Api.Integration.Infrastructure;
using AnalyticsService.Application.Dtos.v1.Responses;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace AnalyticsService.Api.Integration.Controllers.v1;

[TestFixture]
[NonParallelizable]
public class WorkoutAnalyticsControllerIntegrationTests
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
        _factory.WorkoutAnalyticsService.Reset();
        _client = _factory.CreateAuthenticatedClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    [Test]
    public async Task GetSummary_WhenServiceReturnsSuccess_Returns200WithPayload()
    {
        // Arrange
        _factory.WorkoutAnalyticsService.SummaryHandler = _ =>
            Task.FromResult(ResultOfT<WorkoutSummaryResponse>.Success(new WorkoutSummaryResponse
            {
                AmountPerWeek = 5,
                WeekDurationMin = 220,
                AmountThisMonth = 18,
                MonthlyTimeMin = 860
            }));

        // Act
        var response = await _client.GetAsync("/api/v1/workout-analytics/summary");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var payload = await response.Content.ReadFromJsonAsync<WorkoutSummaryResponse>();
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload!.AmountPerWeek, Is.EqualTo(5));
        Assert.That(payload.WeekDurationMin, Is.EqualTo(220));
    }

    [Test]
    public async Task GetLast7DaysActivity_WhenServiceReturnsSuccess_Returns200WithPayload()
    {
        // Arrange
        _factory.WorkoutAnalyticsService.DailyTrendHandler = (days, _) =>
            Task.FromResult(ResultOfT<IReadOnlyList<WorkoutDailyTrendPointResponse>>.Success(
                new List<WorkoutDailyTrendPointResponse>
                {
                    new() { Date = new DateTime(2026, 4, 15), WorkoutsCount = days, DurationMin = 50 }
                }));

        // Act
        var response = await _client.GetAsync("/api/v1/workout-analytics/daily/last-7-days");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_factory.WorkoutAnalyticsService.LastDailyTrendDays, Is.EqualTo(7));

        var payload = await response.Content.ReadFromJsonAsync<List<WorkoutDailyTrendPointResponse>>();
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload, Has.Count.EqualTo(1));
        Assert.That(payload![0].WorkoutsCount, Is.EqualTo(7));
    }

    [Test]
    public async Task GetCountsByType_WithFromAndTo_UsesQueryAndReturns200()
    {
        // Arrange
        var from = new DateTime(2026, 4, 1);
        var to = new DateTime(2026, 4, 21);

        // Act
        var response = await _client.GetAsync($"/api/v1/workout-analytics/by-type?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_factory.WorkoutAnalyticsService.LastFrom, Is.EqualTo(from));
        Assert.That(_factory.WorkoutAnalyticsService.LastTo, Is.EqualTo(to));
    }

    [Test]
    public async Task GetStatisticsOverview_WhenServiceReturnsSuccess_Returns200WithPayload()
    {
        // Arrange
        _factory.WorkoutAnalyticsService.StatisticsOverviewHandler = _ =>
            Task.FromResult(ResultOfT<WorkoutStatisticsOverviewResponse>.Success(new WorkoutStatisticsOverviewResponse
            {
                TotalAchievedGoals = 12,
                TotalTrainingHours = 48.5,
                TotalWorkoutsCompleted = 70,
                WorkoutsThisWeek = 6
            }));

        // Act
        var response = await _client.GetAsync("/api/v1/workout-analytics/statistics-overview");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var payload = await response.Content.ReadFromJsonAsync<WorkoutStatisticsOverviewResponse>();
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload!.TotalAchievedGoals, Is.EqualTo(12));
        Assert.That(payload.TotalWorkoutsCompleted, Is.EqualTo(70));
    }

    [Test]
    public async Task GetSummary_WhenServiceReturnsEntityNotFound_Returns404()
    {
        // Arrange
        _factory.WorkoutAnalyticsService.SummaryHandler = _ =>
            Task.FromResult(ResultOfT<WorkoutSummaryResponse>.Failure(new Error(ErrorCode.EntityNotFound,
                "Workout summary was not found.")));

        // Act
        var response = await _client.GetAsync("/api/v1/workout-analytics/summary");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        var payload = await response.Content.ReadAsStringAsync();
        Assert.That(payload, Does.Contain("Workout summary was not found."));
    }

    [Test]
    [TestCase("/api/v1/workout-analytics/summary")]
    [TestCase("/api/v1/workout-analytics/daily/last-7-days")]
    [TestCase("/api/v1/workout-analytics/by-type")]
    [TestCase("/api/v1/workout-analytics/statistics-overview")]
    public async Task Endpoints_WhenNoAuthHeader_Return401(string route)
    {
        // Arrange
        using var anonymousClient = _factory.CreateClient();

        // Act
        var response = await anonymousClient.GetAsync(route);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
