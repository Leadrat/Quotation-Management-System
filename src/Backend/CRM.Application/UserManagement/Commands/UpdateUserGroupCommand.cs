using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Commands;

public class UpdateUserGroupCommand
{
    public Guid GroupId { get; set; }
    public UpdateUserGroupRequest Request { get; set; } = null!;
    public Guid UpdatedByUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

