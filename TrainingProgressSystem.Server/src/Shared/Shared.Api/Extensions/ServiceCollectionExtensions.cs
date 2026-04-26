using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Shared.Api.OData;
using Shared.Abstractions.Caching;
using Shared.Abstractions.Data;
using Shared.Abstractions.Idempotency;
using Shared.Api.Idempotency;
using Shared.Caching.Services;
using Shared.Infrastructure.Identity;
using Shared.Infrastructure.Idempotency;
using Microsoft.OpenApi;

namespace Shared.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureFluentValidation(this IServiceCollection services, params Type[] markerTypes)
    {
        if (markerTypes is null || markerTypes.Length == 0)
        {
            throw new ArgumentException("At least one validator marker type must be provided.", nameof(markerTypes));
        }

        var assemblies = markerTypes
            .Select(type => type.Assembly)
            .Distinct()
            .ToArray();

        services.ConfigureFluentValidation(assemblies);
    }

    public static void ConfigureFluentValidation(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies is null || assemblies.Length == 0)
        {
            throw new ArgumentException("At least one assembly must be provided for validator registration.", nameof(assemblies));
        }

        services.AddFluentValidationAutoValidation();

        foreach (var assembly in assemblies.Distinct())
        {
            services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        }
    }

    public static void ConfigureGrpc(this IServiceCollection services, WebApplicationBuilder builder,
        IConfiguration configuration, ILogger logger, int fallbackPort = 8081)
    {
        services.AddGrpc();
        logger.LogInformation("Configured gRPC services");
    }

    public static IMvcBuilder ConfigureControllersWithOData(this IServiceCollection services)
    {
        return services
            .AddControllers()
            .AddOData(options =>
            {
                options.Select().Filter().OrderBy().Count().SetMaxTop(ODataDefaults.MaxTop);
            });
    }

    public static void ConfigureApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new QueryStringApiVersionReader("api-version"),
                new HeaderApiVersionReader("X-API-Version"),
                new MediaTypeApiVersionReader("api-version"),
                new UrlSegmentApiVersionReader()
            );
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
    }
    
    public static void ConfigureProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                var httpContext = context.HttpContext;

                context.ProblemDetails.Instance =
                    httpContext.Request.Path;

                context.ProblemDetails.Extensions["traceId"] =
                    httpContext.TraceIdentifier;
            };
        });
    }

    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme."
            });

            options.AddSecurityRequirement(document =>
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
        });
    }

    public static void ConfigureCaching(this IServiceCollection services, IConfiguration configuration, string applicationName)
    {
        var redisConnectionString = configuration["Redis:ConnectionString"];
        
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = applicationName;
            });

            services.AddTransient<ICacheService, DistributedCacheService>();
        }
        else
        {
            // Fallback to in-memory cache if Redis is not configured
            services.AddDistributedMemoryCache();
            services.AddTransient<ICacheService, InMemoryCacheService>();
        }
    }

    public static void ConfigureIdempotency(this IServiceCollection services)
    {
        services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
        services.AddScoped<IIdempotencyService, IdempotencyService>();
    }
    
    public static void ConfigureDbContext<TContext>(this IServiceCollection services, IConfiguration configuration, string connectionStringName)
        where TContext : DbContext, IDbContext
    {
        services.AddDbContext<TContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(connectionStringName)));
        
        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<TContext>());
    }

    public static void ConfigureIdentity<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddIdentityApiEndpoints<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<TContext>()
            .AddDefaultTokenProviders();
        
        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
        });
        
        services.Configure<IdentityOptions>(options =>
        {
            options.SignIn.RequireConfirmedEmail = false;
        });
        
         services.Configure<IdentityOptions>(options =>
        {
            options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = false;
        });
    }
}