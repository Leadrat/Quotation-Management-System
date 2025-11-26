using System;

namespace CRM.Application.UserManagement.Queries;

public class GetUserProfileQuery
{
    public Guid UserId { get; set; }
    public Guid RequestorUserId { get; set; }
}

