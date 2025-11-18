namespace CRM.Domain.Admin;

/// <summary>
/// Rules for data archiving and purging
/// </summary>
public class DataRetentionPolicy
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty; // e.g., "Quotation", "Client", "AuditLog"
    public int RetentionPeriodMonths { get; set; }
    public bool IsActive { get; set; }
    public bool AutoPurgeEnabled { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public Entities.User? CreatedByUser { get; set; }
    public Entities.User? UpdatedByUser { get; set; }
}

