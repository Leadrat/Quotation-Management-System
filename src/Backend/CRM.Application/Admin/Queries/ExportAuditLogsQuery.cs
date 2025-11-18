namespace CRM.Application.Admin.Queries;

/// <summary>
/// Query to export audit logs to CSV
/// </summary>
public class ExportAuditLogsQuery
{
    public Guid? PerformedBy { get; set; }
    public string? ActionType { get; set; }
    public string? Entity { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}

