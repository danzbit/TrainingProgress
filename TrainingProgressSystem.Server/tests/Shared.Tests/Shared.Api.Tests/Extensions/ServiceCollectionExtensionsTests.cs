using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Abstractions.Caching;
using Shared.Abstractions.Data;
using Shared.Abstractions.Idempotency;
using Shared.Api.Extensions;
using Shared.Api.Idempotency;
using Shared.Caching.Services;
using Shared.Contracts.Idempotency;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Idempotency;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shared.Api.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void ConfigureGrpc_RegistersGrpcOptions()
    {
        var builder = WebApplication.CreateBuilder();
        var logger = Mock.Of<ILogger>();

        builder.Services.ConfigureGrpc(builder, builder.Configuration, logger);

        using var provider = builder.Services.BuildServiceProvider();

        Assert.That(provider.GetService<IOptions<GrpcServiceOptions>>(), Is.Not.Null);
    }

    [Test]
    public void ConfigureControllersWithOData_RegistersControllerAndODataServices()
    {
        var services = new ServiceCollection();

        var mvcBuilder = services.ConfigureControllersWithOData();

        Assert.That(mvcBuilder, Is.Not.Null);
        Assert.That(services.Any(descriptor =>
            descriptor.ServiceType.Namespace?.Contains("OData", StringComparison.Ordinal) == true), Is.True);
    }

    [Test]
    public void ConfigureApiVersioning_ConfiguresExpectedOptions()
    {
        var services = new ServiceCollection();

        services.ConfigureApiVersioning();

        using var provider = services.BuildServiceProvider();
        var versioningOptions = provider.GetRequiredService<IOptions<ApiVersioningOptions>>().Value;
        var explorerOptions = provider.GetRequiredService<IOptions<ApiExplorerOptions>>().Value;

        Assert.That(versioningOptions.AssumeDefaultVersionWhenUnspecified, Is.True);
        Assert.That(versioningOptions.DefaultApiVersion, Is.EqualTo(new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0)));
        Assert.That(versioningOptions.ReportApiVersions, Is.True);
        Assert.That(explorerOptions.GroupNameFormat, Is.EqualTo("'v'VVV"));
        Assert.That(explorerOptions.SubstituteApiVersionInUrl, Is.True);
    }

    [Test]
    public void ConfigureProblemDetails_CustomizesInstanceAndTraceId()
    {
        var services = new ServiceCollection();

        services.ConfigureProblemDetails();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ProblemDetailsOptions>>().Value;
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "trace-123";
        httpContext.Request.Path = "/api/test";

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails()
        };

        options.CustomizeProblemDetails?.Invoke(context);

        Assert.That(context.ProblemDetails.Instance, Is.EqualTo("/api/test"));
        Assert.That(context.ProblemDetails.Extensions["traceId"], Is.EqualTo("trace-123"));
    }

    [Test]
    public void ConfigureSwagger_RegistersBearerSecurityScheme()
    {
        var services = new ServiceCollection();

        services.ConfigureSwagger();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<SwaggerGenOptions>>().Value;
        var bearerScheme = options.SwaggerGeneratorOptions.SecuritySchemes["Bearer"];

        Assert.That(bearerScheme.Name, Is.EqualTo("Authorization"));
        Assert.That(bearerScheme.Scheme, Is.EqualTo("bearer"));
        Assert.That(bearerScheme.BearerFormat, Is.EqualTo("JWT"));
        Assert.That(options.SwaggerGeneratorOptions.SecurityRequirements, Is.Not.Empty);
    }

    [Test]
    public void ConfigureCaching_WhenRedisConfigured_RegistersDistributedCacheService()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Redis:ConnectionString"] = "localhost:6379"
            })
            .Build();

        services.ConfigureCaching(configuration, "shared-api");

        var descriptor = services.FirstOrDefault(service => service.ServiceType == typeof(ICacheService));

        Assert.That(descriptor, Is.Not.Null);
        Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(DistributedCacheService)));
    }

    [Test]
    public void ConfigureCaching_WhenRedisMissing_RegistersInMemoryCacheService()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.ConfigureCaching(configuration, "shared-api");

        var descriptor = services.FirstOrDefault(service => service.ServiceType == typeof(ICacheService));

        Assert.That(descriptor, Is.Not.Null);
        Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(InMemoryCacheService)));
        Assert.That(services.Any(service => service.ServiceType == typeof(IDistributedCache)), Is.True);
    }

    [Test]
    public void ConfigureIdempotency_RegistersScopedServices()
    {
        var services = new ServiceCollection();

        services.ConfigureIdempotency();

        AssertScoped<IIdempotencyRepository, IdempotencyRepository>(services);
        AssertScoped<IIdempotencyService, IdempotencyService>(services);
    }

    [Test]
    public void ConfigureDbContext_RegistersConcreteContextAndIdbContext()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\MSSQLLocalDB;Database=SharedApiTests;Trusted_Connection=True;"
            })
            .Build();

        services.ConfigureDbContext<TestDbContext>(configuration, "DefaultConnection");

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var abstraction = scope.ServiceProvider.GetRequiredService<IDbContext>();

        Assert.That(abstraction, Is.SameAs(dbContext));
    }

    [Test]
    public void ConfigureIdentity_ConfiguresIdentityOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<TestDbContext>(options =>
            options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SharedApiIdentityTests;Trusted_Connection=True;"));

        services.ConfigureIdentity<TestDbContext>();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<IdentityOptions>>().Value;

        Assert.That(options.Password.RequireDigit, Is.True);
        Assert.That(options.Password.RequireLowercase, Is.True);
        Assert.That(options.Password.RequireUppercase, Is.True);
        Assert.That(options.Password.RequireNonAlphanumeric, Is.True);
        Assert.That(options.Password.RequiredLength, Is.EqualTo(8));
        Assert.That(options.SignIn.RequireConfirmedEmail, Is.False);
        Assert.That(options.User.RequireUniqueEmail, Is.False);
        Assert.That(options.User.AllowedUserNameCharacters, Does.Contain("-._@+"));
    }

    private static void AssertScoped<TService, TImplementation>(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(service => service.ServiceType == typeof(TService));

        Assert.That(descriptor, Is.Not.Null);
        Assert.That(descriptor!.ImplementationType, Is.EqualTo(typeof(TImplementation)));
        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options)
        : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IDbContext
    {
        public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    }
}
