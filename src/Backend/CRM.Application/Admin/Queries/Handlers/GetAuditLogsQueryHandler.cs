using System.Text.Json;
using CRM.Application.Admin.DTOs;
using CRM.Application.Admin.Services;
using CRM.Domain.Admin;

namespace CRM.Application.Admin.Queries.Handlers;

public class GetAuditLogsQueryHandler
{
    private readonly IAuditLogService _auditLogService;

    public GetAuditLogsQueryHandler(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    public async Task<AuditLogListDto> Handle(GetAuditLogsQuery query)
    {
        var (entries, totalCount) = await _auditLogService.GetAuditLogsAsync(
            performedBy: query.PerformedBy,
            actionType: query.ActionType,
            entity: query.Entity,
            startDate: query.StartDate,
            endDate: query.EndDate,
            pageNumber: query.PageNumber,
            pageSize: query.PageSize);

        var dtos = entries.Select(e => new AuditLogDto
        {
            Id = e.Id,
            ActionType = e.ActionType,
            Entity = e.Entity,
            EntityId = e.EntityId,
            PerformedBy = e.PerformedBy,
            PerformedByEmail = e.PerformedByUser?.Email,
            IpAddress = e.IpAddress,
            Timestamp = e.Timestamp,
            Changes = e.Changes
        }).ToList();

        return new AuditLogListDto
        {
            Entries = dtos,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}

