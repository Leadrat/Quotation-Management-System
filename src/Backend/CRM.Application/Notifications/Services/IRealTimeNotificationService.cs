using System;
using System.Threading.Tasks;
using CRM.Application.Notifications.Dtos;

namespace CRM.Application.Notifications.Services
{
    public interface IRealTimeNotificationService
    {
        Task SendToUserAsync(Guid userId, NotificationDto notification);
        Task SendToGroupAsync(string groupName, NotificationDto notification);
    }
}

