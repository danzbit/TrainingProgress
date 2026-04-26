using System.Net;
using System.Net.Http.Json;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Api.Integration.Infrastructure;
using TrainingService.Application.Dtos.v1.Requests;
using TrainingService.Application.Dtos.v1.Responses;

namespace TrainingService.Api.Integration.Controllers.v1;

[TestFixture]
[NonParallelizable]
public class WorkoutsControllerIntegrationTests
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
        _factory.WorkoutService.Reset();
        _client = _factory.CreateAuthenticatedClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
    }

    [Test]
    public async Task GetAll_WhenAuthenticated_Returns200WithWorkouts()
    {
        // Arrange
        _factory.WorkoutService.GetAllHandler = () => Task.FromResult(
            ResultOfT<IReadOnlyList<WorkoutsListItemResponse>>.Success(new List<WorkoutsListItemResponse>
            {
                new("Strength", 45, 3, new DateTime(2026, 4, 20))
            }));

        // Act
        var response = await _client.GetAsync("/api/v1/workouts");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("Strength"));
    }

    [Test]
    public async Task GetAll_WhenNotAuthenticated_Returns401()
    {
        // Arrange
        using var anonClient = _factory.CreateClient();

        // Act
        var response = await anonClient.GetAsync("/api/v1/workouts");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetAll_WhenServiceReturnsFailure_ReturnsErrorStatus()
    {
        // Arrange
        _factory.WorkoutService.GetAllHandler = () => Task.FromResult(
            ResultOfT<IReadOnlyList<WorkoutsListItemResponse>>.Failure(Error.EntityNotFound));

        // Act
        var response = await _client.GetAsync("/api/v1/workouts");

        // Assert
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetById_WhenWorkoutExists_Returns200WithWorkout()
    {
        // Arrange
        var workoutId = Guid.NewGuid();
        _factory.WorkoutService.GetByIdHandler = id => Task.FromResult(
            ResultOfT<WorkoutsResponse>.Success(new WorkoutsResponse(
                id,
                new DateTime(2026, 4, 20),
                60,
                "Notes",
                new WorkoutTypeResponse(Guid.NewGuid(), "Cardio", null),
                new List<ExerciseResponse>())));

        // Act
        var response = await _client.GetAsync($"/api/v1/workouts/{workoutId}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var payload = await response.Content.ReadFromJsonAsync<WorkoutsResponse>();
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload!.Id, Is.EqualTo(workoutId));
        Assert.That(payload.DurationMin, Is.EqualTo(60));
    }

    [Test]
    public async Task GetById_WhenWorkoutNotFound_Returns404()
    {
        // Arrange
        _factory.WorkoutService.GetByIdHandler = _ => Task.FromResult(
            ResultOfT<WorkoutsResponse>.Failure(Error.EntityNotFound));

        // Act
        var response = await _client.GetAsync($"/api/v1/workouts/{Guid.NewGuid()}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_WhenValid_Returns200()
    {
        // Arrange
        _factory.WorkoutService.UpdateHandler = _ => Task.FromResult(Result.Success());

        var request = new UpdateWorkoutRequest(
            Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow,
            Guid.NewGuid(), 45, "Updated notes", null);

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/workouts", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Delete_WhenWorkoutExists_Returns200()
    {
        // Arrange
        _factory.WorkoutService.DeleteHandler = _ => Task.FromResult(Result.Success());

        // Act
        var response = await _client.DeleteAsync($"/api/v1/workouts/{Guid.NewGuid()}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Delete_WhenWorkoutNotFound_Returns404()
    {
        // Arrange
        _factory.WorkoutService.DeleteHandler = _ => Task.FromResult(Result.Failure(Error.EntityNotFound));

        // Act
        var response = await _client.DeleteAsync($"/api/v1/workouts/{Guid.NewGuid()}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
