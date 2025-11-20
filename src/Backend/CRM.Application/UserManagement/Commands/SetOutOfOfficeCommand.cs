using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Commands;

public class SetOutOfOfficeCommand
{
    public Guid UserId { get; set; }
    public SetOutOfOfficeRequest Request { get; set; } = null!;
    public Guid UpdatedByUserId { get; set; }
}

