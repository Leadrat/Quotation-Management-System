using CRM.Domain.Entities;

namespace CRM.Application.Notifications.Services
{
    public interface INotificationTemplate
    {
        string GetSubject(UserNotification notification);
        string GetBody(UserNotification notification);
    }
}

