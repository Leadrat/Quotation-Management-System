using CRM.Domain.Enums;

namespace CRM.Application.Notifications.Services
{
    // TODO: Implement template classes for each event type
    public class NotificationEventTemplateService : INotificationEventTemplateService
    {
        public INotificationTemplate GetTemplate(NotificationEventType eventType)
        {
            // For now, return a default template until specific templates are implemented
            return new DefaultNotificationTemplate();
        }
    }
}

