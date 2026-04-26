using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Application.Dtos.v1.Requests;
using AuthService.Application.Dtos.v1.Responses;
using AuthService.Application.Interfaces.v1;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared.Infrastructure.Identity;
using Shared.Kernal.Results;
using Shared.Kernals.Errors;

namespace AuthService.Application.Services.v1;

public class AuthService(
    IConfiguration configuration,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : IAuthService
{
    public async Task<ResultOfT<List<string>>> Register(RegistrationRequest userModel)
    {
        if (userModel.Email is not null)
        {
            var userExist = await userManager.FindByEmailAsync(userModel.Email);

            if (userExist is not null)
            {
                return ResultOfT<List<string>>.Failure(AuthErrors.UserAlreadyExist);
            }
        }

        var user = new ApplicationUser
        {
            UserName = userModel.UserName,
            Email = userModel.Email,
        };

        if (userModel.Password is not null)
        {
            var result = await userManager.CreateAsync(user, userModel.Password);

            if (!result.Succeeded)
            {
                return ResultOfT<List<string>>.Failure(result.Errors.Select(e => e.Description).ToList(),
                    AuthErrors.UserCreation);
            }
        }

        return ResultOfT<List<string>>.Success();
    }

    public async Task<ResultOfT<LoginResponse>> Login(LoginRequest loginRequest)
    {
        if (loginRequest.UserNameOrEmail is null)
        {
            return ResultOfT<LoginResponse>.Failure();
        }

        var user = await userManager.FindByEmailAsync(loginRequest.UserNameOrEmail) ??
                   await userManager.FindByNameAsync(loginRequest.UserNameOrEmail);

        if (user is null)
        {
            return ResultOfT<LoginResponse>.Failure(AuthErrors.UserNotFound);
        }

        if (loginRequest.Password is not null)
        {
            var result = await signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, false);

            if (!result.Succeeded)
            {
                return ResultOfT<LoginResponse>.Failure(AuthErrors.Unauthorized);
            }
        }

        var token = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user);

        await SaveRefreshToken(user, refreshToken);

        return ResultOfT<LoginResponse>.Success(new LoginResponse(user.Id, token, refreshToken));
    }

    public async Task<ResultOfT<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return ResultOfT<RefreshTokenResponse>.Failure(AuthErrors.RefreshTokenRequired);
        }

        var principal = GetPrincipalFromExpiredToken(request.RefreshToken);
        if (principal == null)
        {
            return ResultOfT<RefreshTokenResponse>.Failure(AuthErrors.InvalidRefreshToken);
        }

        var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                 ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return ResultOfT<RefreshTokenResponse>.Failure(AuthErrors.InvalidTokenClaims);
        }

        var user = await userManager.FindByIdAsync(parsedUserId.ToString());
        if (user == null)
        {
            return ResultOfT<RefreshTokenResponse>.Failure(AuthErrors.UserNotFound);
        }

        if (user.RefreshToken != request.RefreshToken || 
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return ResultOfT<RefreshTokenResponse>.Failure(AuthErrors.InvalidRefreshToken);
        }

        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken(user);

        await SaveRefreshToken(user, newRefreshToken);

        return ResultOfT<RefreshTokenResponse>.Success(
            new RefreshTokenResponse(user.Id, newAccessToken, newRefreshToken));
    }

    public async Task<ResultOfT<GetCurrentUserResponse>> GetCurrentUser(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ResultOfT<GetCurrentUserResponse>.Failure(AuthErrors.UserNotFound);
        }

        return ResultOfT<GetCurrentUserResponse>.Success(
            new GetCurrentUserResponse(user.Id, user.UserName!, user.Email!));
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"] ?? string.Empty)),
                ValidateLifetime = false,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateAccessToken(IdentityUser<Guid> user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        };

        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"] ?? string.Empty)),
                SecurityAlgorithms.HmacSha256)
        );

        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(token);

        return encodedJwt;
    }

    private string GenerateRefreshToken(IdentityUser<Guid> user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"] ?? string.Empty)),
                SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task SaveRefreshToken(ApplicationUser user, string refreshToken)
    {
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);
    }
}
