using System.Net;
using System.Net.Http.Json;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Api.Integration.Infrastructure;
using TrainingService.Application.Dtos.v1.Responses;

namespace TrainingService.Api.Integration.Controllers.v1;

[TestFixture]
[NonParallelizable]
public class WorkoutTypesControllerIntegrationTests
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
        _factory.WorkoutTypeService.Reset();
        _client = _factory.CreateAuthenticatedClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
    }

    [Test]
    public async Task GetAll_WhenAuthenticated_Returns200WithWorkoutTypes()
    {
        // Arrange
        _factory.WorkoutTypeService.GetAllHandler = () => Task.FromResult(
            ResultOfT<IReadOnlyList<WorkoutTypeResponse>>.Success(new List<WorkoutTypeResponse>
            {
                new(Guid.NewGuid(), "Strength", "Strength training"),
                new(Guid.NewGuid(), "Yoga", "Flexibility and mindfulness")
            }));

        // Act
        var response = await _client.GetAsync("/api/v1/workout-types");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var payload = await response.Content.ReadFromJsonAsync<List<WorkoutTypeResponse>>();
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload, Has.Count.EqualTo(2));
        Assert.That(payload![0].Name, Is.EqualTo("Strength"));
    }

    [Test]
    public async Task GetAll_WhenNotAuthenticated_Returns401()
    {
        // Arrange
        using var anonClient = _factory.CreateClient();

        // Act
        var response = await anonClient.GetAsync("/api/v1/workout-types");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetAll_WhenServiceReturnsEmpty_Returns200WithEmptyList()
    {
        // Arrange
        _factory.WorkoutTypeService.GetAllHandler = () => Task.FromResult(
            ResultOfT<IReadOnlyList<WorkoutTypeResponse>>.Success(
                new List<WorkoutTypeResponse>()));

        // Act
        var response = await _client.GetAsync("/api/v1/workout-types");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var payload = await response.Content.ReadFromJsonAsync<List<WorkoutTypeResponse>>();
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload, Is.Empty);
    }

    [Test]
    public async Task GetAll_WhenServiceFails_ReturnsErrorStatus()
    {
        // Arrange
        _factory.WorkoutTypeService.GetAllHandler = () => Task.FromResult(
            ResultOfT<IReadOnlyList<WorkoutTypeResponse>>.Failure(Error.UnexpectedError));

        // Act
        var response = await _client.GetAsync("/api/v1/workout-types");

        // Assert
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.OK));
    }
}
