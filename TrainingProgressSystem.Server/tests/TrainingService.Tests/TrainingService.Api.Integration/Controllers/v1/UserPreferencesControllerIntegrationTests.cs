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
public class UserPreferencesControllerIntegrationTests
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
        _factory.UserPreferenceService.Reset();
        _client = _factory.CreateAuthenticatedClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
    }

    [Test]
    public async Task Get_WhenAuthenticated_Returns200WithPreferences()
    {
        // Arrange
        _factory.UserPreferenceService.GetPreferenceHandler = _ => Task.FromResult(
            ResultOfT<UserPreferenceResponse>.Success(new UserPreferenceResponse("Grid")));

        // Act
        var response = await _client.GetAsync("/api/v1/userpreferences");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var payload = await response.Content.ReadFromJsonAsync<UserPreferenceResponse>();
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload!.HistoryViewMode, Is.EqualTo("Grid"));
    }

    [Test]
    public async Task Get_WhenNotAuthenticated_Returns401()
    {
        // Arrange
        using var anonClient = _factory.CreateClient();

        // Act
        var response = await anonClient.GetAsync("/api/v1/userpreferences");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Get_WhenServiceFails_ReturnsErrorStatus()
    {
        // Arrange
        _factory.UserPreferenceService.GetPreferenceHandler = _ => Task.FromResult(
            ResultOfT<UserPreferenceResponse>.Failure(Error.EntityNotFound));

        // Act
        var response = await _client.GetAsync("/api/v1/userpreferences");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_WhenValid_Returns200()
    {
        // Arrange
        _factory.UserPreferenceService.UpdatePreferenceHandler = (_, _) => Task.FromResult(Result.Success());

        var request = new UpdateUserPreferenceRequest("List");

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/userpreferences", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Update_WhenNotAuthenticated_Returns401()
    {
        // Arrange
        using var anonClient = _factory.CreateClient();
        var request = new UpdateUserPreferenceRequest("List");

        // Act
        var response = await anonClient.PutAsJsonAsync("/api/v1/userpreferences", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Update_WhenServiceFails_ReturnsErrorStatus()
    {
        // Arrange
        _factory.UserPreferenceService.UpdatePreferenceHandler = (_, _) => Task.FromResult(
            Result.Failure(Error.UnexpectedError));

        var request = new UpdateUserPreferenceRequest("InvalidMode");

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/userpreferences", request);

        // Assert
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.OK));
    }
}
