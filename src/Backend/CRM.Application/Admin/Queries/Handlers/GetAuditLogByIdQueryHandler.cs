using CRM.Application.Admin.DTOs;
using CRM.Application.Admin.Services;

namespace CRM.Application.Admin.Queries.Handlers;

public class GetAuditLogByIdQueryHandler
{
    private readonly IAuditLogService _auditLogService;

    public GetAuditLogByIdQueryHandler(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    public async Task<AuditLogDto?> Handle(GetAuditLogByIdQuery query)
    {
        var entry = await _auditLogService.GetByIdAsync(query.Id);
        if (entry == null) return null;

        return new AuditLogDto
        {
            Id = entry.Id,
            ActionType = entry.ActionType,
            Entity = entry.Entity,
            EntityId = entry.EntityId,
            PerformedBy = entry.PerformedBy,
            PerformedByEmail = entry.PerformedByUser?.Email,
            IpAddress = entry.IpAddress,
            Timestamp = entry.Timestamp,
            Changes = entry.Changes
        };
    }
}

