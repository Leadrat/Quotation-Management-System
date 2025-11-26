using System;

namespace CRM.Application.UserManagement.Queries;

public class GetActivityFeedQuery
{
    public Guid? UserId { get; set; }
    public string? ActionType { get; set; }
    public string? EntityType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid RequestorUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

