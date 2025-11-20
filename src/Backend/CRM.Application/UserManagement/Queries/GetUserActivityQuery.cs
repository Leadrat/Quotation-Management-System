using System;

namespace CRM.Application.UserManagement.Queries;

public class GetUserActivityQuery
{
    public Guid UserId { get; set; }
    public string? ActionType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid RequestorUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

