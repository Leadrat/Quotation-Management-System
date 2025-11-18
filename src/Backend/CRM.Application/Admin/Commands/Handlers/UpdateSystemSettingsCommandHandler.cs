using System.Text.Json;
using CRM.Application.Admin.DTOs;
using CRM.Application.Admin.Services;
using CRM.Application.Common.Persistence;

namespace CRM.Application.Admin.Commands.Handlers;

public class UpdateSystemSettingsCommandHandler
{
    private readonly ISystemSettingsService _settingsService;
    private readonly IAuditLogService _auditLogService;
    private readonly IAppDbContext _db;

    public UpdateSystemSettingsCommandHandler(
        ISystemSettingsService settingsService,
        IAuditLogService auditLogService,
        IAppDbContext db)
    {
        _settingsService = settingsService;
        _auditLogService = auditLogService;
        _db = db;
    }

    public async Task<SystemSettingsDto> Handle(UpdateSystemSettingsCommand command)
    {
        // Get old values for audit log
        var oldSettings = await _settingsService.GetAllSettingsAsync();

        // Update settings
        await _settingsService.UpdateSettingsAsync(command.Settings, command.ModifiedBy);

        // Get new values for audit log
        var newSettings = await _settingsService.GetAllSettingsAsync();

        // Create audit log entry
        var changes = JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            before = oldSettings,
            after = newSettings
        }));

        await _auditLogService.LogAsync(
            actionType: "SettingsUpdated",
            entity: "SystemSettings",
            entityId: null,
            performedBy: command.ModifiedBy,
            ipAddress: command.IpAddress,
            changes: changes);

        return new SystemSettingsDto { Settings = newSettings };
    }
}

