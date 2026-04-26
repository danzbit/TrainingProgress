using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces.v1;
using NotificationService.Application.Services.v1;

namespace NotificationService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureApplicationServices(this IServiceCollection service, ILogger logger)
    {
        service.AddScoped<IRemindersService, RemindersService>();
        service.AddScoped<INotificationSyncService, NotificationSyncService>();
        logger.LogInformation("Registered application services");

        logger.LogInformation("Configured application services");
    }
}