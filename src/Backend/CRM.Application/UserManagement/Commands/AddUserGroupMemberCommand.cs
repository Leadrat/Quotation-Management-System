using System;

namespace CRM.Application.UserManagement.Commands;

public class AddUserGroupMemberCommand
{
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public Guid AddedByUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

