namespace AuthService.Application.Dtos.v1.Responses;

public record GetCurrentUserResponse(Guid Id, string UserName, string Email);