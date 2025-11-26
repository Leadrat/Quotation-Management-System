using CRM.Domain.Entities;
using CRM.Domain.Enums;

namespace CRM.Application.Notifications.Repositories;

public interface INotificationTemplateRepository
{
    Task<NotificationTemplate?> GetByIdAsync(int id);
    Task<NotificationTemplate?> GetByTemplateKeyAsync(string templateKey);
    Task<List<NotificationTemplate>> GetByChannelAsync(NotificationChannel channel, bool activeOnly = true);
    Task<List<NotificationTemplate>> GetAllAsync(bool activeOnly = true);
    Task<NotificationTemplate> CreateAsync(NotificationTemplate template);
    Task<NotificationTemplate> UpdateAsync(NotificationTemplate template);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(string templateKey);
    Task<List<NotificationTemplate>> SearchAsync(string searchTerm, NotificationChannel? channel = null, bool activeOnly = true);
    Task<List<NotificationTemplate>> GetByEventTypeAsync(string eventType);
    Task<List<string>> GetVariablesForEventTypeAsync(string eventType);
}