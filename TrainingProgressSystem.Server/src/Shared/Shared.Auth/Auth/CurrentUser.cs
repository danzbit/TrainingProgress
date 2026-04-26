using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.Auth;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace Shared.Auth.Auth;

public sealed class CurrentUser(IHttpContextAccessor http, ILogger<CurrentUser> logger) : ICurrentUser
{
    private bool IsAuthenticated => http.HttpContext?.User?.Identity?.IsAuthenticated == true;

    private string? UserId =>
        http.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? http.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public ResultOfT<Guid> GetCurrentUserId()
    {
        if (!IsAuthenticated || string.IsNullOrWhiteSpace(UserId))
        {
            logger.LogWarning("Cannot get workouts for unauthenticated user");
            return ResultOfT<Guid>.Failure(new Error(ErrorCode.UnexpectedError, "User is not authenticated."));
        }

        if (!Guid.TryParse(UserId, out var userId))
        {
            logger.LogWarning("Failed to parse current user id: {UserId}", UserId);
            return ResultOfT<Guid>.Failure(
                new Error(ErrorCode.DeserializationFailed, "Current user id is invalid."));
        }

        return ResultOfT<Guid>.Success(userId);
    }
}