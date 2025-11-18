namespace CRM.Application.Admin.Requests;

/// <summary>
/// Request to update data retention policy
/// </summary>
public class UpdateDataRetentionPolicyRequest
{
    public string EntityType { get; set; } = string.Empty;
    public int RetentionPeriodMonths { get; set; }
    public bool IsActive { get; set; }
    public bool AutoPurgeEnabled { get; set; }
}

