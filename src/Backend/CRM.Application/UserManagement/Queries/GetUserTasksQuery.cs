using System;

namespace CRM.Application.UserManagement.Queries;

public class GetUserTasksQuery
{
    public Guid UserId { get; set; }
    public string? Status { get; set; }
    public string? EntityType { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid RequestorUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

