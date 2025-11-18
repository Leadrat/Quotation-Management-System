using CRM.Domain.Enums;
using CRM.Domain.Entities;

namespace CRM.Application.Notifications.Services
{
    public interface INotificationTemplateService
    {
        INotificationTemplate GetTemplate(NotificationEventType eventType);
    }
}

