using System;
using System.Collections.Generic;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string? PhoneCode { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid RoleId { get; set; }
    public Guid? ReportingManagerId { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int LoginAttempts { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Enhanced profile properties
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? Skills { get; set; } // JSONB array of strings
    public bool OutOfOfficeStatus { get; set; } = false;
    public string? OutOfOfficeMessage { get; set; }
    public Guid? DelegateUserId { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public PresenceStatus PresenceStatus { get; set; } = PresenceStatus.Offline;

    public virtual Role? Role { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual User? ReportingManager { get; set; }
    public virtual ICollection<User> DirectReports { get; set; } = new List<User>();
    public virtual User? DelegateUser { get; set; }
    public virtual ICollection<User> DelegatedToMe { get; set; } = new List<User>();

    public string GetFullName() => string.Join(" ", new[] { FirstName, LastName }).Trim();

    public void SetOutOfOffice(bool isOutOfOffice, string? message = null, Guid? delegateUserId = null)
    {
        OutOfOfficeStatus = isOutOfOffice;
        OutOfOfficeMessage = message;
        DelegateUserId = delegateUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePresence(PresenceStatus status)
    {
        PresenceStatus = status;
        LastSeenAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLastSeen()
    {
        LastSeenAt = DateTime.UtcNow;
    }

    public void SetSkills(List<string> skills)
    {
        if (skills == null || skills.Count == 0)
        {
            Skills = "[]";
        }
        else
        {
            Skills = System.Text.Json.JsonSerializer.Serialize(skills);
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public List<string> GetSkills()
    {
        if (string.IsNullOrWhiteSpace(Skills))
        {
            return new List<string>();
        }

        try
        {
            var skills = System.Text.Json.JsonSerializer.Deserialize<List<string>>(Skills);
            return skills ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}
