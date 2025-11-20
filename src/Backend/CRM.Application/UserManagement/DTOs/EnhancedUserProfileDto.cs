using System;
using System.Collections.Generic;
using CRM.Domain.Enums;

namespace CRM.Application.UserManagement.DTOs;

public class EnhancedUserProfileDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public List<string> Skills { get; set; } = new();
    public bool OutOfOfficeStatus { get; set; }
    public string? OutOfOfficeMessage { get; set; }
    public Guid? DelegateUserId { get; set; }
    public string? DelegateUserName { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public PresenceStatus PresenceStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

