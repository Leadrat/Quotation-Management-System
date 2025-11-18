using System.Text.Json;
using CRM.Application.Admin.Services;
using CRM.Application.Common.Persistence;

namespace CRM.Application.Admin.Commands.Handlers;

public class DeleteIntegrationKeyCommandHandler
{
    private readonly IIntegrationKeyService _keyService;
    private readonly IAuditLogService _auditLogService;
    private readonly IAppDbContext _db;

    public DeleteIntegrationKeyCommandHandler(
        IIntegrationKeyService keyService,
        IAuditLogService auditLogService,
        IAppDbContext db)
    {
        _keyService = keyService;
        _auditLogService = auditLogService;
        _db = db;
    }

    public async Task<bool> Handle(DeleteIntegrationKeyCommand command)
    {
        // Get key info before deletion for audit
        var key = await _keyService.GetKeyByIdAsync(command.Id);
        if (key == null) return false;

        var deleted = await _keyService.DeleteKeyAsync(command.Id);
        if (!deleted) return false;

        // Log to audit
        var changes = JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            keyName = key.KeyName,
            provider = key.Provider,
            action = "deleted"
        }));

        await _auditLogService.LogAsync(
            actionType: "IntegrationKeyDeleted",
            entity: "IntegrationKey",
            entityId: command.Id,
            performedBy: command.DeletedBy,
            ipAddress: command.IpAddress,
            changes: changes);

        return true;
    }
}

