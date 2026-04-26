using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Abstractions.Auth;
using Shared.Auth.Auth;
using Shared.Auth.Extensions;

namespace Shared.Auth.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void ConfigureCurrentUser_RegistersExpectedServices()
    {
        var services = new ServiceCollection();

        services.ConfigureCurrentUser();

        var currentUserDescriptor = services.FirstOrDefault(service => service.ServiceType == typeof(ICurrentUser));
        var httpContextAccessorDescriptor = services.FirstOrDefault(service => service.ServiceType == typeof(IHttpContextAccessor));

        Assert.That(currentUserDescriptor, Is.Not.Null);
        Assert.That(currentUserDescriptor!.ImplementationType, Is.EqualTo(typeof(CurrentUser)));
        Assert.That(currentUserDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
        Assert.That(httpContextAccessorDescriptor, Is.Not.Null);
        Assert.That(httpContextAccessorDescriptor!.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
    }

    [Test]
    public async Task ConfigureJwtAuthentication_ConfiguresBearerOptionsAndCookieFallback()
    {
        const string secretKey = "super-secret-signing-key-1234567890";
        const string issuer = "training-progress";
        const string audience = "training-progress-clients";
        const string cookieToken = "cookie.jwt.token";

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = secretKey,
                ["JwtSettings:Issuer"] = issuer,
                ["JwtSettings:Audience"] = audience,
            })
            .Build();

        services.ConfigureJwtAuthentication(configuration);

        using var provider = services.BuildServiceProvider();
        var authenticationOptions = provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
        var authorizationOptions = provider.GetService<IOptions<Microsoft.AspNetCore.Authorization.AuthorizationOptions>>();
        var jwtOptions = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.That(authenticationOptions.DefaultAuthenticateScheme, Is.EqualTo(JwtBearerDefaults.AuthenticationScheme));
        Assert.That(authenticationOptions.DefaultChallengeScheme, Is.EqualTo(JwtBearerDefaults.AuthenticationScheme));
        Assert.That(authorizationOptions, Is.Not.Null);
        Assert.That(jwtOptions.MapInboundClaims, Is.False);
        Assert.That(jwtOptions.TokenValidationParameters.ValidateIssuerSigningKey, Is.True);
        Assert.That(jwtOptions.TokenValidationParameters.ValidIssuer, Is.EqualTo(issuer));
        Assert.That(jwtOptions.TokenValidationParameters.ValidAudience, Is.EqualTo(audience));
        Assert.That(jwtOptions.TokenValidationParameters.ValidateLifetime, Is.True);
        Assert.That(jwtOptions.TokenValidationParameters.ClockSkew, Is.EqualTo(TimeSpan.Zero));

        var signingKey = jwtOptions.TokenValidationParameters.IssuerSigningKey as SymmetricSecurityKey;

        Assert.That(signingKey, Is.Not.Null);
        Assert.That(signingKey!.Key, Is.EqualTo(Encoding.UTF8.GetBytes(secretKey)));

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = string.Empty;
        httpContext.Request.Headers.Cookie = $"accessToken={cookieToken}";

        var messageReceivedContext = new MessageReceivedContext(
            httpContext,
            new AuthenticationScheme(JwtBearerDefaults.AuthenticationScheme, null, typeof(JwtBearerHandler)),
            jwtOptions);

        await jwtOptions.Events.OnMessageReceived(messageReceivedContext);

        Assert.That(messageReceivedContext.Token, Is.EqualTo(cookieToken));
    }
}