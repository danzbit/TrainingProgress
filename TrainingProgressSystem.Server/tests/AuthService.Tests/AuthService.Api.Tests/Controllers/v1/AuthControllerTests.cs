using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.Api.Controllers.v1;
using AuthService.Application.Dtos.v1.Requests;
using AuthService.Application.Dtos.v1.Responses;
using AuthService.Application.Interfaces.v1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace AuthService.Api.Tests.Controllers.v1;

[TestFixture]
public class AuthControllerTests
{
    private Mock<IAuthService> _authServiceMock = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _authServiceMock = new Mock<IAuthService>(MockBehavior.Strict);
        _controller = new AuthController(
            _authServiceMock.Object,
            LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Warning)).CreateLogger<AuthController>());

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Test]
    public async Task Register_WhenServiceSucceeds_ReturnsOkObjectResult()
    {
        var request = new RegistrationRequest
        {
            UserName = "new.user",
            Email = "new.user@example.com",
            Password = "Password123!"
        };

        _authServiceMock
            .Setup(service => service.Register(request))
            .ReturnsAsync(ResultOfT<List<string>>.Success(["Registration completed."]));

        var result = await _controller.Register(request);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(new List<string> { "Registration completed." }));
        _authServiceMock.Verify(service => service.Register(request), Times.Once);
    }

    [Test]
    public async Task Register_WhenServiceReturnsIdentityValidationErrors_ReturnsBadRequestWithValidationProblemDetails()
    {
        var request = new RegistrationRequest
        {
            UserName = "new.user",
            Email = "new.user@example.com",
            Password = "weak"
        };

        _authServiceMock
            .Setup(service => service.Register(request))
            .ReturnsAsync(ResultOfT<List<string>>.Failure(
                [
                    "Passwords must have at least one uppercase ('A'-'Z').",
                    "Passwords must have at least one non alphanumeric character."
                ],
                new Error(ErrorCode.UserCreation, "Failed to create user.")));

        var result = await _controller.Register(request);

        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest, Is.Not.Null);

        var payload = badRequest!.Value;
        Assert.That(payload, Is.Not.Null);

        var detail = payload!.GetType().GetProperty("detail")?.GetValue(payload) as string;
        var errors = payload.GetType().GetProperty("errors")?.GetValue(payload) as Dictionary<string, string[]>;

        Assert.That(detail, Is.EqualTo("Failed to create user."));
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors!["errors"], Has.Member("Passwords must have at least one uppercase ('A'-'Z')."));
        Assert.That(errors["errors"], Has.Member("Passwords must have at least one non alphanumeric character."));
    }

    [Test]
    public async Task Login_WhenServiceSucceeds_ReturnsOkAndSetsCookies()
    {
        var request = new LoginRequest
        {
            UserNameOrEmail = "new.user",
            Password = "Password123!"
        };

        _authServiceMock
            .Setup(service => service.Login(request))
            .ReturnsAsync(ResultOfT<LoginResponse>.Success(
                new LoginResponse(Guid.NewGuid(), "access-token", "refresh-token")));

        var result = await _controller.Login(request);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var cookies = _controller.Response.Headers.SetCookie.ToArray();
        Assert.That(cookies, Has.Some.Contains("accessToken=access-token"));
        Assert.That(cookies, Has.Some.Contains("refreshToken=refresh-token"));
        _authServiceMock.Verify(service => service.Login(request), Times.Once);
    }

    [Test]
    public async Task Login_WhenServiceFails_DoesNotSetCookies()
    {
        var request = new LoginRequest
        {
            UserNameOrEmail = "missing.user",
            Password = "Password123!"
        };

        _authServiceMock
            .Setup(service => service.Login(request))
            .ReturnsAsync(ResultOfT<LoginResponse>.Failure(new Error(ErrorCode.EntityNotFound, "User not found")));

        var result = await _controller.Login(request);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        Assert.That(_controller.Response.Headers.SetCookie, Is.Empty);
    }

    [Test]
    public async Task RefreshToken_WhenBodyTokenProvided_PrefersBodyTokenAndSetsCookies()
    {
        _controller.Request.Headers.Cookie = "refreshToken=cookie-token";
        _authServiceMock
            .Setup(service => service.RefreshToken(It.Is<RefreshTokenRequest>(token => token.RefreshToken == "body-token")))
            .ReturnsAsync(ResultOfT<RefreshTokenResponse>.Success(
                new RefreshTokenResponse(Guid.NewGuid(), "new-access", "new-refresh")));

        var result = await _controller.RefreshToken(new RefreshTokenRequest { RefreshToken = "body-token" });

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var cookies = _controller.Response.Headers.SetCookie.ToArray();
        Assert.That(cookies, Has.Some.Contains("accessToken=new-access"));
        Assert.That(cookies, Has.Some.Contains("refreshToken=new-refresh"));
        _authServiceMock.Verify(service => service.RefreshToken(
            It.Is<RefreshTokenRequest>(token => token.RefreshToken == "body-token")), Times.Once);
    }

    [Test]
    public async Task RefreshToken_WhenTokenMissingFromBody_UsesCookieValue()
    {
        _controller.Request.Headers.Cookie = "refreshToken=cookie-token";
        _authServiceMock
            .Setup(service => service.RefreshToken(It.Is<RefreshTokenRequest>(token => token.RefreshToken == "cookie-token")))
            .ReturnsAsync(ResultOfT<RefreshTokenResponse>.Success(
                new RefreshTokenResponse(Guid.NewGuid(), "new-access", "new-refresh")));

        var result = await _controller.RefreshToken(new RefreshTokenRequest());

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        _authServiceMock.Verify(service => service.RefreshToken(
            It.Is<RefreshTokenRequest>(token => token.RefreshToken == "cookie-token")), Times.Once);
    }

    [Test]
    public async Task RefreshToken_WhenTokenMissingFromBodyAndCookie_ReturnsBadRequest()
    {
        var result = await _controller.RefreshToken(new RefreshTokenRequest());

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        _authServiceMock.Verify(service => service.RefreshToken(It.IsAny<RefreshTokenRequest>()), Times.Never);
    }

    [Test]
    public async Task GetCurrentUser_WhenValidSubClaimExists_ReturnsOkObjectResult()
    {
        var userId = Guid.NewGuid();
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())
        ], "Test"));

        _authServiceMock
            .Setup(service => service.GetCurrentUser(userId))
            .ReturnsAsync(ResultOfT<GetCurrentUserResponse>.Success(
                new GetCurrentUserResponse(userId, "new.user", "new.user@example.com")));

        var result = await _controller.GetCurrentUser();

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        _authServiceMock.Verify(service => service.GetCurrentUser(userId), Times.Once);
    }

    [Test]
    public async Task GetCurrentUser_WhenUserIdClaimIsInvalid_ReturnsUnauthorized()
    {
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "not-a-guid")
        ], "Test"));

        var result = await _controller.GetCurrentUser();

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        _authServiceMock.Verify(service => service.GetCurrentUser(It.IsAny<Guid>()), Times.Never);
    }
}