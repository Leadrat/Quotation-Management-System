using System;

namespace CRM.Application.UserManagement.Queries;

public class GetUserGroupByIdQuery
{
    public Guid GroupId { get; set; }
    public Guid RequestorUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

