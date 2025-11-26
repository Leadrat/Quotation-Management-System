using CRM.Domain.Entities;
using CRM.Domain.Enums;

namespace CRM.Application.Notifications.Services
{
    public interface INotificationDispatchService
    {
        Task DispatchNotificationAsync(Guid notificationId);
        Task DispatchNotificationAsync(UserNotification notification);
        Task<bool> CanDispatchToChannelAsync(NotificationChannel channel);
        Task<IEnumerable<NotificationDispatchAttempt>> GetDispatchHistoryAsync(Guid notificationId);
        Task RetryFailedDispatchAsync(int dispatchAttemptId);
    }
}