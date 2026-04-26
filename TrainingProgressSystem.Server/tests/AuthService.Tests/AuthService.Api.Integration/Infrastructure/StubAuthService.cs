using AuthService.Application.Dtos.v1.Requests;
using AuthService.Application.Dtos.v1.Responses;
using AuthService.Application.Interfaces.v1;
using Shared.Kernal.Results;

namespace AuthService.Api.Integration.Infrastructure;

public sealed class StubAuthService : IAuthService
{
    public Func<RegistrationRequest, Task<ResultOfT<List<string>>>> RegisterHandler { get; set; } =
        _ => Task.FromResult(ResultOfT<List<string>>.Success(["Registration completed."]));

    public Func<LoginRequest, Task<ResultOfT<LoginResponse>>> LoginHandler { get; set; } =
        _ => Task.FromResult(ResultOfT<LoginResponse>.Success(
            new LoginResponse(Guid.NewGuid(), "access-token", "refresh-token")));

    public Func<RefreshTokenRequest, Task<ResultOfT<RefreshTokenResponse>>> RefreshTokenHandler { get; set; } =
        request => Task.FromResult(ResultOfT<RefreshTokenResponse>.Success(
            new RefreshTokenResponse(Guid.NewGuid(), $"access-for-{request.RefreshToken}", $"refresh-for-{request.RefreshToken}")));

    public Func<Guid, Task<ResultOfT<GetCurrentUserResponse>>> GetCurrentUserHandler { get; set; } =
        userId => Task.FromResult(ResultOfT<GetCurrentUserResponse>.Success(
            new GetCurrentUserResponse(userId, "integration.user", "integration@example.com")));

    public RegistrationRequest? LastRegistrationRequest { get; private set; }

    public LoginRequest? LastLoginRequest { get; private set; }

    public RefreshTokenRequest? LastRefreshTokenRequest { get; private set; }

    public Guid? LastGetCurrentUserId { get; private set; }

    public int RegisterCallCount { get; private set; }

    public int LoginCallCount { get; private set; }

    public int RefreshTokenCallCount { get; private set; }

    public int GetCurrentUserCallCount { get; private set; }

    public Task<ResultOfT<List<string>>> Register(RegistrationRequest userModel)
    {
        RegisterCallCount++;
        LastRegistrationRequest = userModel;
        return RegisterHandler(userModel);
    }

    public Task<ResultOfT<LoginResponse>> Login(LoginRequest loginRequest)
    {
        LoginCallCount++;
        LastLoginRequest = loginRequest;
        return LoginHandler(loginRequest);
    }

    public Task<ResultOfT<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request)
    {
        RefreshTokenCallCount++;
        LastRefreshTokenRequest = request;
        return RefreshTokenHandler(request);
    }

    public Task<ResultOfT<GetCurrentUserResponse>> GetCurrentUser(Guid userId)
    {
        GetCurrentUserCallCount++;
        LastGetCurrentUserId = userId;
        return GetCurrentUserHandler(userId);
    }

    public void Reset()
    {
        RegisterHandler = _ => Task.FromResult(ResultOfT<List<string>>.Success(["Registration completed."]));
        LoginHandler = _ => Task.FromResult(ResultOfT<LoginResponse>.Success(
            new LoginResponse(Guid.NewGuid(), "access-token", "refresh-token")));
        RefreshTokenHandler = request => Task.FromResult(ResultOfT<RefreshTokenResponse>.Success(
            new RefreshTokenResponse(Guid.NewGuid(), $"access-for-{request.RefreshToken}", $"refresh-for-{request.RefreshToken}")));
        GetCurrentUserHandler = userId => Task.FromResult(ResultOfT<GetCurrentUserResponse>.Success(
            new GetCurrentUserResponse(userId, "integration.user", "integration@example.com")));

        LastRegistrationRequest = null;
        LastLoginRequest = null;
        LastRefreshTokenRequest = null;
        LastGetCurrentUserId = null;
        RegisterCallCount = 0;
        LoginCallCount = 0;
        RefreshTokenCallCount = 0;
        GetCurrentUserCallCount = 0;
    }
}