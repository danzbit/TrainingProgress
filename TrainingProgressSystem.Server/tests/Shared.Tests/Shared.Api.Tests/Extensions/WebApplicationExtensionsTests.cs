using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Api.Extensions;
using Shared.Api.Middlewares;

namespace Shared.Api.Tests.Extensions;

[TestFixture]
public class WebApplicationExtensionsTests
{
    [Test]
    public void ConfigureWebApplication_DoesNotThrow_WhenRequiredServicesAreRegistered()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddControllers();
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

        var app = builder.Build();

        Assert.DoesNotThrow(() => app.ConfigureWebApplication());
    }

    [Test]
    public void ConfigureSwagger_DoesNotThrow_WhenSwaggerServicesAreRegistered()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        Assert.DoesNotThrow(() => app.ConfigureSwagger());
    }

    [Test]
    public void ConfigureApiMiddlewares_DoesNotThrow_WhenExceptionHandlerIsConfigured()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        var app = builder.Build();

        Assert.DoesNotThrow(() => app.ConfigureApiMiddlewares());
    }
}