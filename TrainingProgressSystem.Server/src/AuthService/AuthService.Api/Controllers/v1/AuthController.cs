using AuthService.Application.Dtos.v1.Requests;
using AuthService.Application.Interfaces.v1;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernal.Models;

namespace AuthService.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("1.0")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [Route("sign-up")]
    public async Task<IActionResult> Register(RegistrationRequest userModel)
    {
        logger.LogInformation("Received registration request for username: {Username}", userModel.UserName);
        var result = await authService.Register(userModel);

        return result.ToActionResult();
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("sign-in")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        logger.LogInformation("Received login request for username: {Username}", loginRequest.UserNameOrEmail);
        var result = await authService.Login(loginRequest);

        if (!result.IsFailure && result.Value != null)
        {
            SetAuthCookie("accessToken", result.Value.Token);
            if (!string.IsNullOrEmpty(result.Value.RefreshToken))
            {
                SetAuthCookie("refreshToken", result.Value.RefreshToken);
            }
        }

        return result.ToActionResult();
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest? request)
    {
        logger.LogInformation("Received refresh token request");

        // Prefer the token from the request body; fall back to the HTTP-only cookie sent by the browser.
        var refreshToken = request?.RefreshToken;
        if (string.IsNullOrEmpty(refreshToken))
        {
            refreshToken = Request.Cookies["refreshToken"];
        }

        if (string.IsNullOrEmpty(refreshToken))
        {
            logger.LogWarning("Refresh token missing from both request body and cookie");
            return BadRequest(new { message = "Refresh token is required." });
        }

        var result = await authService.RefreshToken(new RefreshTokenRequest { RefreshToken = refreshToken });

        if (!result.IsFailure && result.Value != null)
        {
            SetAuthCookie("accessToken", result.Value.AccessToken);
            SetAuthCookie("refreshToken", result.Value.RefreshToken);
        }

        return result.ToActionResult();
    }

    [HttpGet]
    [Authorize]
    [Route("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        logger.LogInformation("Received get current user request");

        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            logger.LogWarning("Invalid user ID in token claims");
            return Unauthorized();
        }

        var result = await authService.GetCurrentUser(userId);

        return result.ToActionResult();
    }

    /// <summary>
    /// Sets an HTTP-only cookie with the specified name and token value
    /// </summary>
    private void SetAuthCookie(string cookieName, string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // Prevents JavaScript access (XSS protection)
            Secure = true,   // HTTPS only in production
            SameSite = SameSiteMode.Strict, // CSRF protection
        };

        if (cookieName == "refreshToken")
        {
            cookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(7);
        }
        else
        {
            cookieOptions.Expires = DateTimeOffset.UtcNow.AddHours(1);
        }

        Response.Cookies.Append(cookieName, token, cookieOptions);
    }
}