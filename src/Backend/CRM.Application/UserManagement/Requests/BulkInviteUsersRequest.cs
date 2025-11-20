using System.Collections.Generic;

namespace CRM.Application.UserManagement.Requests;

public class BulkInviteUsersRequest
{
    public List<BulkInviteUserItem> Users { get; set; } = new();
    public Guid? RoleId { get; set; }
    public Guid? TeamId { get; set; }
    public bool SendEmailInvites { get; set; } = true;
}

public class BulkInviteUserItem
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Mobile { get; set; }
}

