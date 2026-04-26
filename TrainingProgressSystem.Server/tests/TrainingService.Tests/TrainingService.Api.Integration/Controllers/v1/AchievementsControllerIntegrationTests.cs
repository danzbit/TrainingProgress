using System.Net;
using System.Net.Http.Json;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;
using TrainingService.Api.Integration.Infrastructure;
using TrainingService.Application.Dtos.v1.Responses;

namespace TrainingService.Api.Integration.Controllers.v1;

[TestFixture]
[NonParallelizable]
public class AchievementsControllerIntegrationTests
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
        _factory.AchievementService.Reset();
        _client = _factory.CreateAuthenticatedClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
    }

    [Test]
    public async Task ShareProgress_WhenAuthenticated_Returns200WithPublicKey()
    {
        // Arrange
        _factory.AchievementService.ShareProgressHandler = _ => Task.FromResult(
            ResultOfT<ShareProgressResponse>.Success(new ShareProgressResponse("abc-key-xyz")));

        // Act
        var response = await _client.PostAsync("/api/v1/achievements/share", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var payload = await response.Content.ReadFromJsonAsync<ShareProgressResponse>();
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload!.PublicUrlKey, Is.EqualTo("abc-key-xyz"));
    }

    [Test]
    public async Task ShareProgress_WhenNotAuthenticated_Returns401()
    {
        // Arrange
        using var anonClient = _factory.CreateClient();

        // Act
        var response = await anonClient.PostAsync("/api/v1/achievements/share", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShareProgress_WhenServiceFails_ReturnsErrorStatus()
    {
        // Arrange
        _factory.AchievementService.ShareProgressHandler = _ => Task.FromResult(
            ResultOfT<ShareProgressResponse>.Failure(Error.UnexpectedError));

        // Act
        var response = await _client.PostAsync("/api/v1/achievements/share", null);

        // Assert
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetSharedProgress_WithValidKey_Returns200WithProgress()
    {
        // Arrange
        var key = "valid-key-123";
        _factory.AchievementService.GetSharedProgressHandler = (k, _) => Task.FromResult(
            ResultOfT<SharedProgressResponse>.Success(
                new SharedProgressResponse("Weekly Progress", "Great week!", DateTime.UtcNow.AddDays(-1), null)));

        // Act
        var response = await _client.GetAsync($"/api/v1/achievements/shared/{key}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var payload = await response.Content.ReadFromJsonAsync<SharedProgressResponse>();
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload!.Title, Is.EqualTo("Weekly Progress"));
    }

    [Test]
    public async Task GetSharedProgress_WhenNotAuthenticated_Returns200()
    {
        // Arrange (AllowAnonymous endpoint)
        using var anonClient = _factory.CreateClient();
        _factory.AchievementService.GetSharedProgressHandler = (_, _) => Task.FromResult(
            ResultOfT<SharedProgressResponse>.Success(
                new SharedProgressResponse("Public Progress", null, DateTime.UtcNow, null)));

        // Act
        var response = await anonClient.GetAsync("/api/v1/achievements/shared/public-key");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetSharedProgress_WhenKeyNotFound_Returns404()
    {
        // Arrange
        _factory.AchievementService.GetSharedProgressHandler = (_, _) => Task.FromResult(
            ResultOfT<SharedProgressResponse>.Failure(Error.EntityNotFound));

        // Act
        var response = await _client.GetAsync("/api/v1/achievements/shared/nonexistent-key");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
