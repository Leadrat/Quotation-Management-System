using CRM.Domain.Entities;
using CRM.Domain.Enums;

namespace CRM.Application.Notifications.Repositories;

public interface INotificationDispatchRepository
{
    Task<NotificationDispatchAttempt?> GetByIdAsync(int id);
    Task<List<NotificationDispatchAttempt>> GetByNotificationIdAsync(Guid notificationId);
    Task<List<NotificationDispatchAttempt>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 20);
    Task<List<NotificationDispatchAttempt>> GetFailedAttemptsAsync(NotificationChannel? channel = null, int pageNumber = 1, int pageSize = 20);
    Task<List<NotificationDispatchAttempt>> GetPermanentlyFailedAttemptsAsync(NotificationChannel? channel = null, int pageNumber = 1, int pageSize = 20);
    Task<List<NotificationDispatchAttempt>> GetDispatchHistoryAsync(
        Guid? notificationId = null,
        Guid? userId = null,
        NotificationChannel? channel = null,
        DispatchStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 20);
    Task<int> GetDispatchHistoryCountAsync(
        Guid? notificationId = null,
        Guid? userId = null,
        NotificationChannel? channel = null,
        DispatchStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
    Task<NotificationDispatchAttempt> CreateAsync(NotificationDispatchAttempt attempt);
    Task<NotificationDispatchAttempt> UpdateAsync(NotificationDispatchAttempt attempt);
    Task DeleteAsync(int id);
    Task<Dictionary<string, int>> GetDispatchStatisticsByChannelAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<Dictionary<string, int>> GetDispatchStatisticsByStatusAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<double> GetAverageDeliveryTimeAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<int> GetTotalAttemptsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<int> GetSuccessfulDeliveriesAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<int> GetFailedAttemptsCountAsync(DateTime? fromDate = null, DateTime? toDate = null);
}