using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Commands;

public class BulkInviteUsersCommand
{
    public BulkInviteUsersRequest Request { get; set; } = null!;
    public Guid InvitedByUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

