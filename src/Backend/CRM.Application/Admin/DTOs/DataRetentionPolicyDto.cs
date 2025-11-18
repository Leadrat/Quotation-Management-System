namespace CRM.Application.Admin.DTOs;

/// <summary>
/// DTO for data retention policy
/// </summary>
public class DataRetentionPolicyDto
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int RetentionPeriodMonths { get; set; }
    public bool IsActive { get; set; }
    public bool AutoPurgeEnabled { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}

