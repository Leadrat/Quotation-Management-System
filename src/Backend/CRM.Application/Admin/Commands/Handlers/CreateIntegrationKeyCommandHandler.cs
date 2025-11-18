using System.Text.Json;
using CRM.Application.Admin.DTOs;
using CRM.Application.Admin.Services;
using CRM.Application.Common.Persistence;

namespace CRM.Application.Admin.Commands.Handlers;

public class CreateIntegrationKeyCommandHandler
{
    private readonly IIntegrationKeyService _keyService;
    private readonly IAuditLogService _auditLogService;
    private readonly IAppDbContext _db;

    public CreateIntegrationKeyCommandHandler(
        IIntegrationKeyService keyService,
        IAuditLogService auditLogService,
        IAppDbContext db)
    {
        _keyService = keyService;
        _auditLogService = auditLogService;
        _db = db;
    }

    public async Task<IntegrationKeyDto> Handle(CreateIntegrationKeyCommand command)
    {
        var result = await _keyService.CreateKeyAsync(
            command.KeyName,
            command.KeyValue,
            command.Provider,
            command.CreatedBy);

        // Log to audit
        var changes = JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            keyName = command.KeyName,
            provider = command.Provider,
            action = "created"
        }));

        await _auditLogService.LogAsync(
            actionType: "IntegrationKeyCreated",
            entity: "IntegrationKey",
            entityId: result.Id,
            performedBy: command.CreatedBy,
            ipAddress: command.IpAddress,
            changes: changes);

        return result;
    }
}

