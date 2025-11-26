using System;

namespace CRM.Application.UserManagement.Queries;

public class GetTeamMembersQuery
{
    public Guid TeamId { get; set; }
    public Guid RequestorUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

