using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Commands;

public class BulkUpdateUsersCommand
{
    public BulkUpdateUsersRequest Request { get; set; } = null!;
    public Guid UpdatedByUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

