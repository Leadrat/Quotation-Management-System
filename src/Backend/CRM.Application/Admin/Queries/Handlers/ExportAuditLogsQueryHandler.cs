using System.Text;
using CRM.Application.Admin.Services;
using CRM.Domain.Admin;

namespace CRM.Application.Admin.Queries.Handlers;

public class ExportAuditLogsQueryHandler
{
    private readonly IAuditLogService _auditLogService;

    public ExportAuditLogsQueryHandler(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    public async Task<byte[]> Handle(ExportAuditLogsQuery query)
    {
        // Get all matching logs (no pagination for export)
        var (entries, _) = await _auditLogService.GetAuditLogsAsync(
            performedBy: query.PerformedBy,
            actionType: query.ActionType,
            entity: query.Entity,
            startDate: query.StartDate,
            endDate: query.EndDate,
            pageNumber: 1,
            pageSize: int.MaxValue); // Get all

        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("Id,ActionType,Entity,EntityId,PerformedBy,PerformedByEmail,IpAddress,Timestamp,Changes");

        // Rows
        foreach (var entry in entries)
        {
            var changes = entry.Changes?.RootElement.GetRawText() ?? "";
            // Escape quotes and wrap in quotes
            changes = changes.Replace("\"", "\"\"");
            
            csv.AppendLine($"{entry.Id},\"{entry.ActionType}\",\"{entry.Entity}\",{entry.EntityId},\"{entry.PerformedBy}\",\"{entry.PerformedByUser?.Email ?? ""}\",\"{entry.IpAddress ?? ""}\",\"{entry.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{changes}\"");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }
}

