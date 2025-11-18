using System.Text.Json;
using CRM.Application.Admin.DTOs;
using CRM.Application.Admin.Services;
using CRM.Application.Common.Persistence;

namespace CRM.Application.Admin.Commands.Handlers;

public class UpdateNotificationSettingsCommandHandler
{
    private readonly INotificationSettingsService _settingsService;
    private readonly IAuditLogService _auditLogService;
    private readonly IAppDbContext _db;

    public UpdateNotificationSettingsCommandHandler(
        INotificationSettingsService settingsService,
        IAuditLogService auditLogService,
        IAppDbContext db)
    {
        _settingsService = settingsService;
        _auditLogService = auditLogService;
        _db = db;
    }

    public async Task<NotificationSettingsDto> Handle(UpdateNotificationSettingsCommand command)
    {
        // Get old settings for audit
        var oldSettings = await _settingsService.GetSettingsAsync();

        var result = await _settingsService.UpdateSettingsAsync(
            command.BannerMessage,
            command.BannerType,
            command.IsVisible,
            command.UpdatedBy);

        // Log to audit
        var changes = JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            before = oldSettings != null ? new
            {
                oldSettings.BannerType,
                oldSettings.IsVisible,
                hasBannerMessage = !string.IsNullOrEmpty(oldSettings.BannerMessage)
            } : null,
            after = new
            {
                result.BannerType,
                result.IsVisible,
                hasBannerMessage = !string.IsNullOrEmpty(result.BannerMessage)
            },
            action = "updated"
        }));

        await _auditLogService.LogAsync(
            actionType: "NotificationSettingsUpdated",
            entity: "NotificationSettings",
            entityId: result.Id,
            performedBy: command.UpdatedBy,
            ipAddress: command.IpAddress,
            changes: changes);

        return result;
    }
}

