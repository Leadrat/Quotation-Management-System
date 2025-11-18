namespace CRM.Application.Admin.Queries;

/// <summary>
/// Query to get a specific audit log entry by ID
/// </summary>
public class GetAuditLogByIdQuery
{
    public Guid Id { get; set; }
}

