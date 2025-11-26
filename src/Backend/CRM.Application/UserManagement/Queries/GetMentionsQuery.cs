using System;

namespace CRM.Application.UserManagement.Queries;

public class GetMentionsQuery
{
    public Guid UserId { get; set; }
    public bool? IsRead { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid RequestorUserId { get; set; }
}

