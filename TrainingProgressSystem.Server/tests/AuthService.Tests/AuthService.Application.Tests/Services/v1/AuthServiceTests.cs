using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Application.Dtos.v1.Requests;
using AuthService.Application.Services.v1;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Shared.Infrastructure.Identity;
using Shared.Kernal.Errors;

namespace AuthService.Application.Tests.Services.v1;

[TestFixture]
public class AuthServiceTests
{
    private Mock<UserManager<ApplicationUser>> _userManager = null!;
    private TestSignInManager _signInManager = null!;
    private IConfiguration _configuration = null!;
    private AuthService.Application.Services.v1.AuthService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Issuer"] = "TestsIssuer",
                ["JwtSettings:Audience"] = "TestsAudience",
                ["JwtSettings:SecretKey"] = "tests-secret-key-min-32-characters"
            })
            .Build();

        _userManager = CreateUserManagerMock();
        _signInManager = new TestSignInManager(_userManager.Object);

        _service = new AuthService.Application.Services.v1.AuthService(
            _configuration,
            _userManager.Object,
            _signInManager);
    }

    [Test]
    public async Task Register_WhenUserAlreadyExists_ReturnsUserAlreadyExistFailure()
    {
        _userManager
            .Setup(manager => manager.FindByEmailAsync("existing@example.com"))
            .ReturnsAsync(new ApplicationUser { Id = Guid.NewGuid(), Email = "existing@example.com" });

        var result = await _service.Register(new RegistrationRequest
        {
            UserName = "existing",
            Email = "existing@example.com",
            Password = "Password123!"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.UserAlreadyExist));
        _userManager.Verify(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Register_WhenCreateFails_ReturnsDescriptionsAndUserCreationError()
    {
        _userManager
            .Setup(manager => manager.FindByEmailAsync("new@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        _userManager
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), "Password123!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        var result = await _service.Register(new RegistrationRequest
        {
            UserName = "new.user",
            Email = "new@example.com",
            Password = "Password123!"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.UserCreation));
        Assert.That(result.Value, Contains.Item("Password too weak"));
    }

    [Test]
    public async Task Register_WhenCreateSucceeds_ReturnsSuccess()
    {
        _userManager
            .Setup(manager => manager.FindByEmailAsync("new@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        _userManager
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), "Password123!"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.Register(new RegistrationRequest
        {
            UserName = "new.user",
            Email = "new@example.com",
            Password = "Password123!"
        });

        Assert.That(result.IsFailure, Is.False);
        _userManager.Verify(manager => manager.CreateAsync(
            It.Is<ApplicationUser>(user => user.UserName == "new.user" && user.Email == "new@example.com"),
            "Password123!"), Times.Once);
    }

    [Test]
    public async Task Login_WhenUserIsNotFound_ReturnsUserNotFound()
    {
        _userManager
            .Setup(manager => manager.FindByEmailAsync("missing.user"))
            .ReturnsAsync((ApplicationUser?)null);
        _userManager
            .Setup(manager => manager.FindByNameAsync("missing.user"))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _service.Login(new LoginRequest
        {
            UserNameOrEmail = "missing.user",
            Password = "Password123!"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task Login_WhenPasswordIsInvalid_ReturnsUnauthorized()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "new.user", Email = "new@example.com" };

        _userManager
            .Setup(manager => manager.FindByEmailAsync("new.user"))
            .ReturnsAsync((ApplicationUser?)null);
        _userManager
            .Setup(manager => manager.FindByNameAsync("new.user"))
            .ReturnsAsync(user);

        _signInManager.CheckPasswordSignInAsyncHandler = (actualUser, password, lockoutOnFailure) =>
            Task.FromResult(actualUser == user && password == "Password123!" && !lockoutOnFailure
                ? SignInResult.Failed
                : SignInResult.Success);

        var result = await _service.Login(new LoginRequest
        {
            UserNameOrEmail = "new.user",
            Password = "Password123!"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.Unauthorized));
        _userManager.Verify(manager => manager.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Test]
    public async Task Login_WhenCredentialsAreValid_ReturnsTokensAndPersistsRefreshToken()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "new.user", Email = "new@example.com" };

        _userManager
            .Setup(manager => manager.FindByEmailAsync("new.user"))
            .ReturnsAsync((ApplicationUser?)null);
        _userManager
            .Setup(manager => manager.FindByNameAsync("new.user"))
            .ReturnsAsync(user);

        _signInManager.CheckPasswordSignInAsyncHandler = (actualUser, password, lockoutOnFailure) =>
            Task.FromResult(actualUser == user && password == "Password123!" && !lockoutOnFailure
                ? SignInResult.Success
                : SignInResult.Failed);

        _userManager
            .Setup(manager => manager.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.Login(new LoginRequest
        {
            UserNameOrEmail = "new.user",
            Password = "Password123!"
        });

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.Id, Is.EqualTo(user.Id));
        Assert.That(result.Value.RefreshToken, Is.Not.Empty);
        Assert.That(user.RefreshToken, Is.EqualTo(result.Value.RefreshToken));
        Assert.That(user.RefreshTokenExpiryTime, Is.GreaterThan(DateTime.UtcNow.AddDays(6)));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Value.Token);
        Assert.That(jwt.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value, Is.EqualTo(user.Id.ToString()));
        _userManager.Verify(manager => manager.UpdateAsync(user), Times.Once);
    }

    [Test]
    public async Task RefreshToken_WhenTokenMissing_ReturnsRefreshTokenRequired()
    {
        var result = await _service.RefreshToken(new RefreshTokenRequest { RefreshToken = " " });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.RefreshTokenRequired));
    }

    [Test]
    public async Task RefreshToken_WhenTokenIsInvalid_ReturnsInvalidRefreshToken()
    {
        var result = await _service.RefreshToken(new RefreshTokenRequest { RefreshToken = "not-a-token" });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.InvalidRefreshToken));
    }

    [Test]
    public async Task RefreshToken_WhenUserIsNotFound_ReturnsUserNotFound()
    {
        var userId = Guid.NewGuid();
        var token = CreateExpiredToken(userId);

        _userManager
            .Setup(manager => manager.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _service.RefreshToken(new RefreshTokenRequest { RefreshToken = token });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task RefreshToken_WhenStoredTokenDoesNotMatch_ReturnsInvalidRefreshToken()
    {
        var userId = Guid.NewGuid();
        var token = CreateExpiredToken(userId);
        var user = new ApplicationUser
        {
            Id = userId,
            RefreshToken = "different-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        _userManager
            .Setup(manager => manager.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        var result = await _service.RefreshToken(new RefreshTokenRequest { RefreshToken = token });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.InvalidRefreshToken));
    }

    [Test]
    public async Task RefreshToken_WhenStoredTokenIsExpired_ReturnsInvalidRefreshToken()
    {
        var userId = Guid.NewGuid();
        var token = CreateExpiredToken(userId);
        var user = new ApplicationUser
        {
            Id = userId,
            RefreshToken = token,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(-1)
        };

        _userManager
            .Setup(manager => manager.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        var result = await _service.RefreshToken(new RefreshTokenRequest { RefreshToken = token });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.InvalidRefreshToken));
    }

    [Test]
    public async Task RefreshToken_WhenTokenIsValid_ReturnsNewTokensAndPersistsRefreshToken()
    {
        var userId = Guid.NewGuid();
        var token = CreateExpiredToken(userId);
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "new.user",
            Email = "new@example.com",
            RefreshToken = token,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(10)
        };

        _userManager
            .Setup(manager => manager.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _userManager
            .Setup(manager => manager.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.RefreshToken(new RefreshTokenRequest { RefreshToken = token });

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.Id, Is.EqualTo(userId));
        Assert.That(result.Value.AccessToken, Is.Not.Empty);
        Assert.That(result.Value.RefreshToken, Is.Not.EqualTo(token));
        Assert.That(user.RefreshToken, Is.EqualTo(result.Value.RefreshToken));
        _userManager.Verify(manager => manager.UpdateAsync(user), Times.Once);
    }

    [Test]
    public async Task GetCurrentUser_WhenUserIsNotFound_ReturnsUserNotFound()
    {
        var userId = Guid.NewGuid();
        _userManager
            .Setup(manager => manager.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _service.GetCurrentUser(userId);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task GetCurrentUser_WhenUserExists_ReturnsProjectedResponse()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "new.user",
            Email = "new@example.com"
        };

        _userManager
            .Setup(manager => manager.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        var result = await _service.GetCurrentUser(userId);

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value.Id, Is.EqualTo(userId));
        Assert.That(result.Value.UserName, Is.EqualTo("new.user"));
        Assert.That(result.Value.Email, Is.EqualTo("new@example.com"));
    }

    private string CreateExpiredToken(Guid userId)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
        var token = new JwtSecurityToken(
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ],
            expires: DateTime.UtcNow.AddMinutes(-5),
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            Options.Create(new IdentityOptions()),
            Mock.Of<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            null!,
            Mock.Of<ILogger<UserManager<ApplicationUser>>>());
    }

    private sealed class TestSignInManager(UserManager<ApplicationUser> userManager)
        : SignInManager<ApplicationUser>(
            userManager,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
            Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>())
    {
        public Func<ApplicationUser, string, bool, Task<SignInResult>> CheckPasswordSignInAsyncHandler { get; set; } =
            (_, _, _) => Task.FromResult(SignInResult.Success);

        public override Task<SignInResult> CheckPasswordSignInAsync(ApplicationUser user, string password, bool lockoutOnFailure)
        {
            return CheckPasswordSignInAsyncHandler(user, password, lockoutOnFailure);
        }
    }
}