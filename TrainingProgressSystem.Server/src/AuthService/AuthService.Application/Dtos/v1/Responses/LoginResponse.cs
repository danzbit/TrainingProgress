namespace AuthService.Application.Dtos.v1.Responses;

public record LoginResponse(Guid Id, string Token, string RefreshToken);