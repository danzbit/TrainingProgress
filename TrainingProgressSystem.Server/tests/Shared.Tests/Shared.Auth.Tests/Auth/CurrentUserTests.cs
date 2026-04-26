using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Auth.Auth;
using Shared.Kernal.Errors;

namespace Shared.Auth.Tests.Auth;

[TestFixture]
public class CurrentUserTests
{
    [Test]
    public void GetCurrentUserId_WhenUserIsUnauthenticated_ReturnsFailure()
    {
        var currentUser = CreateCurrentUser(new DefaultHttpContext());

        var result = currentUser.GetCurrentUserId();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.UnexpectedError));
        Assert.That(result.Error.Description, Is.EqualTo("User is not authenticated."));
    }

    [Test]
    public void GetCurrentUserId_WhenSubjectClaimIsValidGuid_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext
        {
            User = CreatePrincipal(new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()))
        };
        var currentUser = CreateCurrentUser(httpContext);

        var result = currentUser.GetCurrentUserId();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.EqualTo(userId));
    }

    [Test]
    public void GetCurrentUserId_WhenSubjectClaimMissing_UsesNameIdentifierFallback()
    {
        var userId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext
        {
            User = CreatePrincipal(new Claim(ClaimTypes.NameIdentifier, userId.ToString()))
        };
        var currentUser = CreateCurrentUser(httpContext);

        var result = currentUser.GetCurrentUserId();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.EqualTo(userId));
    }

    [Test]
    public void GetCurrentUserId_WhenClaimIsNotGuid_ReturnsFailure()
    {
        var httpContext = new DefaultHttpContext
        {
            User = CreatePrincipal(new Claim(JwtRegisteredClaimNames.Sub, "not-a-guid"))
        };
        var currentUser = CreateCurrentUser(httpContext);

        var result = currentUser.GetCurrentUserId();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.DeserializationFailed));
        Assert.That(result.Error.Description, Is.EqualTo("Current user id is invalid."));
    }

    private static CurrentUser CreateCurrentUser(HttpContext httpContext)
    {
        return new CurrentUser(
            new HttpContextAccessor { HttpContext = httpContext },
            NullLogger<CurrentUser>.Instance);
    }

    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "TestAuth"));
    }
}