using CRM.Domain.Enums;

namespace CRM.Application.Notifications.Services
{
    public interface INotificationEventTemplateService
    {
        INotificationTemplate GetTemplate(NotificationEventType eventType);
    }
}