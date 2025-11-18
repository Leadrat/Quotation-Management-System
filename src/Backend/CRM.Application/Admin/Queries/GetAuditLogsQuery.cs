namespace CRM.Application.Admin.Queries;

/// <summary>
/// Query to get audit logs with filters
/// </summary>
public class GetAuditLogsQuery
{
    public Guid? PerformedBy { get; set; }
    public string? ActionType { get; set; }
    public string? Entity { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

