using System;

namespace CRM.Application.UserManagement.Queries;

public class GetUnreadMentionsCountQuery
{
    public Guid UserId { get; set; }
    public Guid RequestorUserId { get; set; }
}

