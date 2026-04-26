using AiChatService.Application.Interfaces.v1;
using AiChatService.Application.Services.v1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AiChatService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureApplicationServices(this IServiceCollection services, ILogger logger)
    {
        services.AddScoped<IChatService, ChatService>();
        logger.LogInformation("Registered application services");

        logger.LogInformation("Configured application services");
    }
}
