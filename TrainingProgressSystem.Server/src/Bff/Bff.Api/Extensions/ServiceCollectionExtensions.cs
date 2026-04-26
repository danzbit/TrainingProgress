using System.Text;
using System.Threading.RateLimiting;
using Bff.Api.Services;
using Bff.Api.Validators.v1;
using Bff.Application.Interfaces.v1;
using Bff.Application.Options.v1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Api.Extensions;
using Shared.Api.Middlewares;
using Shared.Auth.Extensions;
using Shared.OpenTelemetry.Extensions;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace Bff.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureBffApiServices(this IServiceCollection services, string serviceName, IConfiguration configuration)
    {
        services.ConfigureFluentValidation(
            typeof(CreateWorkoutSagaCommandValidator),
            typeof(SaveGoalSagaCommandValidator));

        services.Configure<CreateWorkoutSagaOptions>(configuration.GetSection(CreateWorkoutSagaOptions.SectionName));
        services.Configure<SaveGoalSagaOptions>(configuration.GetSection(SaveGoalSagaOptions.SectionName));

        services.ConfigureJwtAuthentication(configuration);

        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.ConfigureApiVersioning();

        services.ConfigureProblemDetails();

        services.ConfigureSwagger();

        services.AddReverseProxy()
            .LoadFromMemory(GetRoutes(), GetClusters())
            .AddTransforms(context =>
            {
                // Inject Authorization: Bearer from the HTTP-only accessToken cookie so that
                // downstream services (which only speak Bearer) can authenticate the request.
                context.AddRequestTransform(transformContext =>
                {
                    if (!transformContext.ProxyRequest.Headers.Contains("Authorization") &&
                        transformContext.HttpContext.Request.Cookies.TryGetValue("accessToken", out var token) &&
                        !string.IsNullOrEmpty(token))
                    {
                        transformContext.ProxyRequest.Headers.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    }
                    return ValueTask.CompletedTask;
                });
            });

        services.ConfigureOpenTelemetry(serviceName);

        services.ConfigureCaching(configuration, serviceName);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter("fixed", limiter =>
            {
                limiter.PermitLimit = 100;
                limiter.Window = TimeSpan.FromSeconds(10);
                limiter.QueueLimit = 20;
                limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });

        services.AddHealthChecks();

        services.AddSignalR();
        services.AddSingleton<ISyncNotifier, SignalRSyncNotifier>();

        // Allow SignalR WebSocket connections to pass the JWT via query string
        services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            var existingOnMessageReceived = options.Events?.OnMessageReceived;
            options.Events ??= new JwtBearerEvents();
            options.Events.OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return existingOnMessageReceived?.Invoke(context) ?? Task.CompletedTask;
            };
        });

        services.AddCors(options =>
        {
            options.AddPolicy("frontend", p =>
                p.WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });
    }

    public static void UseSwaggerJsonDynamicFromRoutes(this IApplicationBuilder app)
    {
        var routes = GetRoutes();
        var logger = app.ApplicationServices.GetService<ILogger>();

        app.Use(async (context, next) =>
        {
            if (context.Request.Path.Value != null && context.Request.Path.Value.EndsWith("/swagger/v1/swagger.json"))
            {
                try
                {
                    var matchedRoute = routes.FirstOrDefault(r =>
                        context.Request.Path.StartsWithSegments(
                            r.Match.Path?.Replace("{**catch-all}", "").TrimEnd('/'),
                            out var remaining));

                    if (matchedRoute != null)
                    {
                        var prefix = matchedRoute.Match.Path?.Replace("{**catch-all}", "").TrimEnd('/');

                        var originalBody = context.Response.Body;
                        using var memStream = new MemoryStream();
                        context.Response.Body = memStream;

                        await next();

                        memStream.Seek(0, SeekOrigin.Begin);
                        var json = await new StreamReader(memStream).ReadToEndAsync();

                        json = json.Replace("\"/", $"\"{prefix}/");

                        var bytes = Encoding.UTF8.GetBytes(json);
                        context.Response.Body = originalBody;
                        context.Response.ContentLength = bytes.Length;
                        context.Response.ContentType = "application/json";
                        await context.Response.Body.WriteAsync(bytes);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogCritical("Swagger JSON rewrite failed: {ExMessage}", ex.Message);
                }
            }

            await next();
        });
    }

    private static List<RouteConfig> GetRoutes()
    {
        return
        [
            new RouteConfig
            {
                RouteId = "training-service-route",
                ClusterId = "training-service-cluster",
                Match = new RouteMatch
                {
                    Path = "/training-service/{**catch-all}"
                },
                Transforms =
                [
                    new Dictionary<string, string> { ["PathRemovePrefix"] = "/training-service" },
                    new Dictionary<string, string> { ["RequestHeadersCopy"] = "true" }
                ],
                Metadata = new Dictionary<string, string> { ["RateLimiterPolicy"] = "fixed" }
            },

            new RouteConfig
            {
                RouteId = "analytics-service-route",
                ClusterId = "analytics-service-cluster",
                Match = new RouteMatch
                {
                    Path = "/analytics-service/{**catch-all}"
                },
                Transforms =
                [
                    new Dictionary<string, string> { ["PathRemovePrefix"] = "/analytics-service" },
                    new Dictionary<string, string> { ["RequestHeadersCopy"] = "true" }
                ],
                Metadata = new Dictionary<string, string> { ["RateLimiterPolicy"] = "fixed" }
            },

            new RouteConfig
            {
                RouteId = "notification-service-route",
                ClusterId = "notification-service-cluster",
                Match = new RouteMatch
                {
                    Path = "/notification-service/{**catch-all}"
                },
                Transforms =
                [
                    new Dictionary<string, string> { ["PathRemovePrefix"] = "/notification-service" },
                    new Dictionary<string, string> { ["RequestHeadersCopy"] = "true" }
                ],
                Metadata = new Dictionary<string, string> { ["RateLimiterPolicy"] = "fixed" }
            },

            new RouteConfig
            {
                RouteId = "auth-service-route",
                ClusterId = "auth-service-cluster",
                Match = new RouteMatch
                {
                    Path = "/auth-service/{**catch-all}"
                },
                Transforms =
                [
                    new Dictionary<string, string> { ["PathRemovePrefix"] = "/auth-service" },
                    new Dictionary<string, string> { ["RequestHeadersCopy"] = "true" }
                ],
                Metadata = new Dictionary<string, string> { ["RateLimiterPolicy"] = "fixed" }
            },

            new RouteConfig
            {
                RouteId = "ai-chat-service-route",
                ClusterId = "ai-chat-service-cluster",
                Match = new RouteMatch
                {
                    Path = "/ai-chat-service/{**catch-all}"
                },
                Transforms =
                [
                    new Dictionary<string, string> { ["PathRemovePrefix"] = "/ai-chat-service" },
                    new Dictionary<string, string> { ["RequestHeadersCopy"] = "true" }
                ],
                Metadata = new Dictionary<string, string> { ["RateLimiterPolicy"] = "fixed" }
            }
        ];
    }

    private static IReadOnlyList<ClusterConfig> GetClusters()
    {
        return new List<ClusterConfig>
        {
            new()
            {
                ClusterId = "training-service-cluster",
                LoadBalancingPolicy = "LeastRequests",
                HealthCheck = new HealthCheckConfig
                {
                    Active = new ActiveHealthCheckConfig
                    {
                        Enabled = true,
                        Interval = TimeSpan.FromSeconds(10),
                        Timeout = TimeSpan.FromSeconds(2),
                        Path = "/health"
                    }
                },
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    { "training-service", new DestinationConfig { Address = GetEnv("TRAINING_SERVICE_URL") } }
                }
            },
            new()
            {
                ClusterId = "analytics-service-cluster",
                LoadBalancingPolicy = "LeastRequests",
                HealthCheck = new HealthCheckConfig
                {
                    Active = new ActiveHealthCheckConfig
                    {
                        Enabled = true,
                        Interval = TimeSpan.FromSeconds(10),
                        Timeout = TimeSpan.FromSeconds(2),
                        Path = "/health"
                    }
                },
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    { "analytics-service", new DestinationConfig { Address = GetEnv("ANALYTICS_SERVICE_URL") } }
                }
            },
            new()
            {
                ClusterId = "notification-service-cluster",
                LoadBalancingPolicy = "LeastRequests",
                HealthCheck = new HealthCheckConfig
                {
                    Active = new ActiveHealthCheckConfig
                    {
                        Enabled = true,
                        Interval = TimeSpan.FromSeconds(10),
                        Timeout = TimeSpan.FromSeconds(2),
                        Path = "/health"
                    }
                },
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    { "notification-service", new DestinationConfig { Address = GetEnv("NOTIFICATION_SERVICE_URL") } }
                }
            },
            new()
            {
                ClusterId = "auth-service-cluster",
                LoadBalancingPolicy = "LeastRequests",
                HealthCheck = new HealthCheckConfig
                {
                    Active = new ActiveHealthCheckConfig
                    {
                        Enabled = true,
                        Interval = TimeSpan.FromSeconds(10),
                        Timeout = TimeSpan.FromSeconds(2),
                        Path = "/health"
                    }
                },
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    { "auth-service", new DestinationConfig { Address = GetEnv("AUTH_SERVICE_URL") } }
                }
            },
            new()
            {
                ClusterId = "ai-chat-service-cluster",
                LoadBalancingPolicy = "LeastRequests",
                HealthCheck = new HealthCheckConfig
                {
                    Active = new ActiveHealthCheckConfig
                    {
                        Enabled = true,
                        Interval = TimeSpan.FromSeconds(10),
                        Timeout = TimeSpan.FromSeconds(2),
                        Path = "/health"
                    }
                },
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    { "ai-chat-service", new DestinationConfig { Address = GetEnv("AI_CHAT_SERVICE_URL") } }
                }
            }
        };
    }

    private static string GetEnv(string name) => Environment.GetEnvironmentVariable(name) ??
                                                 throw new Exception($"Environment variable {name} not set.");
}