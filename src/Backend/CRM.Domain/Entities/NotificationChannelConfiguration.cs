using CRM.Domain.Enums;

namespace CRM.Domain.Entities;

public class NotificationChannelConfiguration
{
    public int Id { get; set; }
    public NotificationChannel Channel { get; set; }
    public bool IsEnabled { get; set; }
    public string Configuration { get; set; } = string.Empty; // JSON configuration
    public int MaxRetryAttempts { get; set; }
    public TimeSpan RetryDelay { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}