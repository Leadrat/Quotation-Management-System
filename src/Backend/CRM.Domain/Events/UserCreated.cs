using System;

namespace CRM.Domain.Events;

public class UserCreated
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public Guid RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
}
