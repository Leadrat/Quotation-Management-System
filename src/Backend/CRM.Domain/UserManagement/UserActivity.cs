using System;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Domain.Entities;

namespace CRM.Domain.UserManagement;

[Table("UserActivities")]
public class UserActivity
{
    public Guid ActivityId { get; set; }
    public Guid UserId { get; set; }
    public string ActionType { get; set; } = string.Empty; // LOGIN, LOGOUT, QUOTATION_CREATED, APPROVAL_GIVEN, etc.
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
}

