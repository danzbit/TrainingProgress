using System.Net;
using System.Net.Http.Json;
using AuthService.Api.Integration.Infrastructure;
using AuthService.Application.Dtos.v1.Requests;
using AuthService.Application.Dtos.v1.Responses;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace AuthService.Api.Integration.Controllers.v1;

[TestFixture]
[NonParallelizable]
public class AuthControllerIntegrationTests
{
    private AuthApiFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new AuthApiFactory();
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
        _factory.AuthService.Reset();
        _factory.IdempotencyService.Reset();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    [Test]
    public async Task Register_WhenServiceReturnsSuccess_Returns200AndPassesRequestThrough()
    {
        var request = new RegistrationRequest
        {
            UserName = "new.user",
            Email = "new.user@example.com",
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/sign-up", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_factory.AuthService.RegisterCallCount, Is.EqualTo(1));
        Assert.That(_factory.AuthService.LastRegistrationRequest?.UserName, Is.EqualTo(request.UserName));
        Assert.That(_factory.AuthService.LastRegistrationRequest?.Email, Is.EqualTo(request.Email));

        var payload = await response.Content.ReadAsStringAsync();
        Assert.That(payload, Does.Contain("Registration completed."));
    }

    [Test]
    public async Task Register_WhenServiceReturnsIdentityValidationErrors_Returns400WithValidationMessages()
    {
        _factory.AuthService.RegisterHandler = _ => Task.FromResult(ResultOfT<List<string>>.Failure(
            [
                "Passwords must have at least one uppercase ('A'-'Z').",
                "Passwords must have at least one non alphanumeric character."
            ],
            new Error(ErrorCode.UserCreation, "Failed to create user.")));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/sign-up", new RegistrationRequest
        {
            UserName = "new.user",
            Email = "new.user@example.com",
            Password = "weak"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var payload = await response.Content.ReadAsStringAsync();
        Assert.That(payload, Does.Contain("Failed to create user."));
        Assert.That(payload, Does.Contain("Passwords must have at least one uppercase ('A'-'Z')."));
        Assert.That(payload, Does.Contain("Passwords must have at least one non alphanumeric character."));
    }

    [Test]
    public async Task Login_WhenServiceReturnsTokens_Returns200AndSetsAuthCookies()
    {
        var userId = Guid.NewGuid();
        _factory.AuthService.LoginHandler = _ => Task.FromResult(ResultOfT<LoginResponse>.Success(
            new LoginResponse(userId, "access-token-123", "refresh-token-456")));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/sign-in", new LoginRequest
        {
            UserNameOrEmail = "new.user",
            Password = "Password123!"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_factory.AuthService.LoginCallCount, Is.EqualTo(1));

        var cookies = response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.ToArray()
            : [];

        Assert.That(cookies, Has.Some.Contains("accessToken=access-token-123"));
        Assert.That(cookies, Has.Some.Contains("refreshToken=refresh-token-456"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain(userId.ToString()));
        Assert.That(body, Does.Contain("access-token-123"));
    }

    [Test]
    public async Task RefreshToken_WhenBodyTokenProvided_PrefersBodyOverCookieAndSetsCookies()
    {
        _client.DefaultRequestHeaders.Add("Cookie", "refreshToken=cookie-token");

        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh-token", new RefreshTokenRequest
        {
            RefreshToken = "body-token"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_factory.AuthService.RefreshTokenCallCount, Is.EqualTo(1));
        Assert.That(_factory.AuthService.LastRefreshTokenRequest?.RefreshToken, Is.EqualTo("body-token"));

        var cookies = response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.ToArray()
            : [];

        Assert.That(cookies, Has.Some.Contains("accessToken=access-for-body-token"));
        Assert.That(cookies, Has.Some.Contains("refreshToken=refresh-for-body-token"));
    }

    [Test]
    public async Task RefreshToken_WhenBodyMissing_UsesCookieToken()
    {
        _client.DefaultRequestHeaders.Add("Cookie", "refreshToken=cookie-token");

        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh-token", new { });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_factory.AuthService.LastRefreshTokenRequest?.RefreshToken, Is.EqualTo("cookie-token"));
    }

    [Test]
    public async Task RefreshToken_WhenBodyAndCookieMissing_Returns400WithoutCallingService()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh-token", new { });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(_factory.AuthService.RefreshTokenCallCount, Is.EqualTo(0));

        var payload = await response.Content.ReadAsStringAsync();
        Assert.That(payload, Does.Contain("Refresh token is required."));
    }

    [Test]
    public async Task GetCurrentUser_WhenAuthenticatedWithValidGuidClaim_Returns200AndUsesClaimUserId()
    {
        var userId = Guid.NewGuid();
        _client.Dispose();
        _client = _factory.CreateAuthenticatedClient(userId);

        var response = await _client.GetAsync("/api/v1/auth/me");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_factory.AuthService.GetCurrentUserCallCount, Is.EqualTo(1));
        Assert.That(_factory.AuthService.LastGetCurrentUserId, Is.EqualTo(userId));

        var payload = await response.Content.ReadAsStringAsync();
        Assert.That(payload, Does.Contain(userId.ToString()));
        Assert.That(payload, Does.Contain("integration.user"));
    }

    [Test]
    public async Task GetCurrentUser_WhenClaimIsNotGuid_Returns401WithoutCallingService()
    {
        _client.Dispose();
        _client = _factory.CreateAuthenticatedClient("not-a-guid");

        var response = await _client.GetAsync("/api/v1/auth/me");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(_factory.AuthService.GetCurrentUserCallCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetCurrentUser_WhenAnonymous_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/auth/me");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Login_WhenServiceReturnsFailure_ReturnsMappedStatusCode()
    {
        _factory.AuthService.LoginHandler = _ => Task.FromResult(ResultOfT<LoginResponse>.Failure(
            new Error(ErrorCode.EntityNotFound, "User was not found.")));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/sign-in", new LoginRequest
        {
            UserNameOrEmail = "missing.user",
            Password = "Password123!"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        var payload = await response.Content.ReadAsStringAsync();
        Assert.That(payload, Does.Contain("User was not found."));
    }
}