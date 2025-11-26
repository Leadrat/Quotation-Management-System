using CRM.Application.Notifications.DTOs;
using CRM.Domain.Entities;
using CRM.Domain.Enums;

namespace CRM.Application.Notifications.Services;

public interface INotificationTemplateService
{
    Task<NotificationTemplate?> GetTemplateAsync(string templateKey);
    Task<RenderedTemplateDto> RenderTemplateAsync(string templateKey, object data, NotificationChannel channel);
    Task<NotificationTemplateDto> CreateTemplateAsync(CreateNotificationTemplateRequest request);
    Task<NotificationTemplateDto> UpdateTemplateAsync(int templateId, UpdateNotificationTemplateRequest request);
    Task DeleteTemplateAsync(int templateId);
    Task<List<NotificationTemplateDto>> GetTemplatesAsync(NotificationChannel? channel = null, bool activeOnly = true);
    Task<List<NotificationTemplateDto>> SearchTemplatesAsync(string searchTerm, NotificationChannel? channel = null, bool activeOnly = true);
    Task<bool> ValidateTemplateAsync(string templateKey, object sampleData);
}