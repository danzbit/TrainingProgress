using AuthService.Application.Dtos.v1.Requests;
using AuthService.Application.Dtos.v1.Responses;
using Shared.Kernal.Results;

namespace AuthService.Application.Interfaces.v1;

public interface IAuthService
{
    Task<ResultOfT<List<string>>> Register(RegistrationRequest userModel);
    
    Task<ResultOfT<LoginResponse>> Login(LoginRequest loginRequest);

    Task<ResultOfT<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request);

    Task<ResultOfT<GetCurrentUserResponse>> GetCurrentUser(Guid userId);
}