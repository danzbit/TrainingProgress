using Shared.Kernal.Errors;

namespace Shared.Kernals.Errors;

public static class AuthErrors
{
    public static readonly Error UserAlreadyExist = new(ErrorCode.UserAlreadyExist, "User already exist.");

    public static readonly Error UserNotFound = new(ErrorCode.UserNotFound, "User not found.");

    public static readonly Error Unauthorized = new(ErrorCode.Unauthorized, "Unauthorized.");
    
    public static readonly Error UserCreation = new(ErrorCode.UserCreation, "Failed to create user.");

    public static readonly Error RefreshTokenRequired = new(ErrorCode.RefreshTokenRequired, "Refresh token is required.");

    public static readonly Error InvalidRefreshToken = new(ErrorCode.InvalidRefreshToken, "Invalid or expired refresh token.");

    public static readonly Error InvalidTokenClaims = new(ErrorCode.InvalidTokenClaims, "Invalid token claims.");
}