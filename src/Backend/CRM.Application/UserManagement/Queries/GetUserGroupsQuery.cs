using System;

namespace CRM.Application.UserManagement.Queries;

public class GetUserGroupsQuery
{
    public Guid? CreatedByUserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid RequestorUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

