using Bff.Api.Hubs;
using Shared.Api.Extensions;

namespace Bff.Api.Extensions;

public static class WebApplicationExtensions
{
    public static void Configure(this WebApplication app)
    {
        app.UseCors("frontend");

        app.ConfigureSwagger();

        app.UseExceptionHandler();
        app.UseHttpsRedirection();

        app.MapHealthChecks("/health");

        app.ConfigureWebApplication();

        app.UseRateLimiter();

        app.UseSwaggerJsonDynamicFromRoutes();

        app.MapHub<SyncHub>("/hubs/sync");

        app.MapReverseProxy();
    }
}