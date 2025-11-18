using System.Text.Json;
using CRM.Application.Admin.DTOs;
using CRM.Application.Admin.Services;
using CRM.Application.Common.Persistence;

namespace CRM.Application.Admin.Commands.Handlers;

public class UpdateDataRetentionPolicyCommandHandler
{
    private readonly IDataRetentionService _retentionService;
    private readonly IAuditLogService _auditLogService;
    private readonly IAppDbContext _db;

    public UpdateDataRetentionPolicyCommandHandler(
        IDataRetentionService retentionService,
        IAuditLogService auditLogService,
        IAppDbContext db)
    {
        _retentionService = retentionService;
        _auditLogService = auditLogService;
        _db = db;
    }

    public async Task<DataRetentionPolicyDto> Handle(UpdateDataRetentionPolicyCommand command)
    {
        // Get old policy for audit
        var oldPolicy = await _retentionService.GetPolicyByEntityTypeAsync(command.EntityType);

        var result = await _retentionService.UpsertPolicyAsync(
            command.EntityType,
            command.RetentionPeriodMonths,
            command.IsActive,
            command.AutoPurgeEnabled,
            command.UpdatedBy);

        // Log to audit
        var changes = JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            before = oldPolicy != null ? new
            {
                oldPolicy.RetentionPeriodMonths,
                oldPolicy.IsActive,
                oldPolicy.AutoPurgeEnabled
            } : null,
            after = new
            {
                result.RetentionPeriodMonths,
                result.IsActive,
                result.AutoPurgeEnabled
            },
            action = "updated"
        }));

        await _auditLogService.LogAsync(
            actionType: "DataRetentionPolicyUpdated",
            entity: "DataRetentionPolicy",
            entityId: result.Id,
            performedBy: command.UpdatedBy,
            ipAddress: command.IpAddress,
            changes: changes);

        return result;
    }
}

