using System;

namespace CRM.Domain.UserManagement.Events;

public class OutOfOfficeStatusChanged
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public bool OutOfOfficeStatus { get; init; }
    public string? OutOfOfficeMessage { get; init; }
    public Guid? DelegateUserId { get; init; }
    public DateTime ChangedAt { get; init; }
}

