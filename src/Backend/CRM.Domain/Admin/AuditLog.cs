using System.Text.Json;
using CRM.Domain.Entities;

namespace CRM.Domain.Admin;

/// <summary>
/// Immutable audit log entry for tracking all system and admin actions
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public Guid PerformedBy { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public JsonDocument? Changes { get; set; }

    // Navigation property
    public User? PerformedByUser { get; set; }
}

