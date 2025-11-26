using System;

namespace CRM.Application.UserManagement.Requests;

public class AddTeamMemberRequest
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = "Member"; // Member, Lead, Admin
}

