using CRM.Application.Admin.DTOs;

namespace CRM.Application.Admin.Commands;

/// <summary>
/// Command to update data retention policy
/// </summary>
public class UpdateDataRetentionPolicyCommand
{
    public string EntityType { get; set; } = string.Empty;
    public int RetentionPeriodMonths { get; set; }
    public bool IsActive { get; set; }
    public bool AutoPurgeEnabled { get; set; }
    public Guid UpdatedBy { get; set; }
    public string? IpAddress { get; set; }
}

