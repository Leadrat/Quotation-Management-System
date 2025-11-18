namespace CRM.Domain.Admin;

/// <summary>
/// Encrypted storage for third-party API credentials
/// </summary>
public class IntegrationKey
{
    public Guid Id { get; set; }
    public string KeyName { get; set; } = string.Empty;
    public string KeyValueEncrypted { get; set; } = string.Empty; // Encrypted value
    public string Provider { get; set; } = string.Empty; // e.g., "Stripe", "Razorpay", "EmailService"
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public Entities.User? CreatedByUser { get; set; }
    public Entities.User? UpdatedByUser { get; set; }
}

