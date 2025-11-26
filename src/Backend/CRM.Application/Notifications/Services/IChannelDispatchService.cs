using CRM.Domain.Entities;
using CRM.Domain.Enums;

namespace CRM.Application.Notifications.Services
{
    public interface IChannelDispatchService
    {
        NotificationChannel Channel { get; }
        Task<bool> CanDispatchAsync(UserNotification notification);
        Task<NotificationDispatchAttempt> DispatchAsync(UserNotification notification);
        Task<bool> IsChannelEnabledAsync();
    }
}