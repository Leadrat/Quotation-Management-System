using System;

namespace CRM.Domain.UserManagement.Events;

public class UserProfileUpdated
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public string? Bio { get; init; }
    public string? LinkedInUrl { get; init; }
    public string? TwitterUrl { get; init; }
    public DateTime UpdatedAt { get; init; }
    public Guid UpdatedByUserId { get; init; }
}

