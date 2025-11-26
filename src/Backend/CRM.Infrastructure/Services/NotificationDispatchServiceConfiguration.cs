using Microsoft.Extensions.DependencyInjection;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Repositories;
using CRM.Application.Notifications.Services;
using CRM.Infrastructure.Repositories;

namespace CRM.Infrastructure.Services;

public static class NotificationDispatchServiceConfiguration
{
    public static IServiceCollection AddNotificationDispatchServices(this IServiceCollection services)
    {
        // Template repositories and services
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        services.AddScoped<INotificationTemplateValidationService, NotificationTemplateValidationService>();
        services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
        
        // Placeholder for additional dispatch services - will be implemented in subsequent tasks
        
        return services;
    }
}