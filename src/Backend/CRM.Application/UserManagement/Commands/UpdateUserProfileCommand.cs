using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Commands;

public class UpdateUserProfileCommand
{
    public Guid UserId { get; set; }
    public UpdateUserProfileRequest Request { get; set; } = null!;
    public Guid UpdatedByUserId { get; set; }
}

