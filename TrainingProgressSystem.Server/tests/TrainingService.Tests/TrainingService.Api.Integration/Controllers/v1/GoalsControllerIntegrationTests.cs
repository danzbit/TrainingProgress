using System.Net;
using System.Net.Http.Json;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Api.Integration.Infrastructure;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;
using TrainingService.Domain.Enums;

namespace TrainingService.Api.Integration.Controllers.v1;

[TestFixture]
[NonParallelizable]
public class GoalsControllerIntegrationTests
{
    private TrainingApiFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new TrainingApiFactory();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _client?.Dispose();
        if (_factory is not null) await _factory.DisposeAsync();
    }

    [SetUp]
    public void SetUp()
    {
        _factory.GoalService.Reset();
        _client = _factory.CreateAuthenticatedClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
    }

    [Test]
    public async Task GetAll_WhenAuthenticated_Returns200WithGoals()
    {
        // Arrange
        var progressInfo = new GoalProgressInfoResponse(2, 40.0, false, DateTime.UtcNow);
        _factory.GoalService.GetAllHandler = () => Task.FromResult(
            ResultOfT<IReadOnlyList<GoalsListItemResponse>>.Success(new List<GoalsListItemResponse>
            {
                new(Guid.NewGuid(), "Weekly workout goal", "Run more",
                    GoalMetricType.WorkoutCount, GoalPeriodType.Weekly,
                    GoalStatus.Active, 5, DateTime.UtcNow.AddDays(-7), null, progressInfo)
            }));

        // Act
        var response = await _client.GetAsync("/api/v1/goals");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("Weekly workout goal"));
    }

    [Test]
    public async Task GetAll_WhenNotAuthenticated_Returns401()
    {
        // Arrange
        using var anonClient = _factory.CreateClient();

        // Act
        var response = await anonClient.GetAsync("/api/v1/goals");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetById_WhenGoalExists_Returns200WithGoal()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        _factory.GoalService.GetByIdHandler = id => Task.FromResult(
            ResultOfT<GoalsResponse>.Success(new GoalsResponse(
                id, Guid.NewGuid(), "Run daily", "Stay fit",
                GoalMetricType.WorkoutCount, GoalPeriodType.Monthly, 20,
                GoalStatus.Active, DateTime.UtcNow.AddDays(-5), null,
                10, 50.0, false, DateTime.UtcNow)));

        // Act
        var response = await _client.GetAsync($"/api/v1/goals/{goalId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var payload = await response.Content.ReadFromJsonAsync<GoalsResponse>();
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload!.Id, Is.EqualTo(goalId));
        Assert.That(payload.Name, Is.EqualTo("Run daily"));
    }

    [Test]
    public async Task GetById_WhenGoalNotFound_Returns404()
    {
        // Arrange
        _factory.GoalService.GetByIdHandler = _ => Task.FromResult(
            ResultOfT<GoalsResponse>.Failure(Error.EntityNotFound));

        // Act
        var response = await _client.GetAsync($"/api/v1/goals/{Guid.NewGuid()}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_WhenValid_Returns200()
    {
        // Arrange
        _factory.GoalService.UpdateHandler = (_, _) => Task.FromResult(Result.Success());

        var request = new UpdateGoalRequest(
            Guid.NewGuid(), Guid.NewGuid(), "Updated goal", "Updated description",
            GoalMetricType.WorkoutCount, GoalPeriodType.Weekly,
            10, DateTime.UtcNow.AddDays(-1), null);

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/goals", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Delete_WhenGoalExists_Returns200()
    {
        // Arrange
        _factory.GoalService.DeleteHandler = (_, _) => Task.FromResult(Result.Success());

        // Act
        var response = await _client.DeleteAsync($"/api/v1/goals/{Guid.NewGuid()}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Delete_WhenGoalNotFound_Returns404()
    {
        // Arrange
        _factory.GoalService.DeleteHandler = (_, _) => Task.FromResult(Result.Failure(Error.EntityNotFound));

        // Act
        var response = await _client.DeleteAsync($"/api/v1/goals/{Guid.NewGuid()}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
