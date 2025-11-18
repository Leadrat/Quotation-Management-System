using System.Text.Json;
using CRM.Application.Admin.DTOs;
using CRM.Application.Admin.Services;
using CRM.Application.Common.Persistence;

namespace CRM.Application.Admin.Commands.Handlers;

public class UpdateIntegrationKeyCommandHandler
{
    private readonly IIntegrationKeyService _keyService;
    private readonly IAuditLogService _auditLogService;
    private readonly IAppDbContext _db;

    public UpdateIntegrationKeyCommandHandler(
        IIntegrationKeyService keyService,
        IAuditLogService auditLogService,
        IAppDbContext db)
    {
        _keyService = keyService;
        _auditLogService = auditLogService;
        _db = db;
    }

    public async Task<IntegrationKeyDto> Handle(UpdateIntegrationKeyCommand command)
    {
        // Get old key for audit
        var oldKey = await _keyService.GetKeyByIdAsync(command.Id);

        var result = await _keyService.UpdateKeyAsync(
            command.Id,
            command.KeyName,
            command.KeyValue,
            command.Provider,
            command.UpdatedBy);

        // Log to audit
        var changes = JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            before = oldKey != null ? new { oldKey.KeyName, oldKey.Provider } : null,
            after = new { result.KeyName, result.Provider },
            action = "updated"
        }));

        await _auditLogService.LogAsync(
            actionType: "IntegrationKeyUpdated",
            entity: "IntegrationKey",
            entityId: result.Id,
            performedBy: command.UpdatedBy,
            ipAddress: command.IpAddress,
            changes: changes);

        return result;
    }
}

