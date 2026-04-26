namespace AuthService.Application.Dtos.v1.Responses;

public record RefreshTokenResponse(Guid Id, string AccessToken, string RefreshToken);
