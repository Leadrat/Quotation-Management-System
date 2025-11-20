using System;
using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Commands;

public class AddTeamMemberCommand
{
    public Guid TeamId { get; set; }
    public AddTeamMemberRequest Request { get; set; } = null!;
    public Guid AddedByUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

