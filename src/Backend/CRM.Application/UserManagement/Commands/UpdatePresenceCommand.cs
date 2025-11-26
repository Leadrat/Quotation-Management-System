using CRM.Domain.Enums;

namespace CRM.Application.UserManagement.Commands;

public class UpdatePresenceCommand
{
    public Guid UserId { get; set; }
    public PresenceStatus Status { get; set; }
}

