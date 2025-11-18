using CRM.Domain.Entities;

namespace CRM.Application.Notifications.Services
{
    public interface INotificationTemplate
    {
        string GetSubject(Notification notification);
        string GetBody(Notification notification);
    }
}

