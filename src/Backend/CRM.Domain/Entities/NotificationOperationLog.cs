namespace CRM.Domain.Entities;

/// <summary>
/// Entity for logging notification operations for monitoring and analysis
/// </summary>
public class NotificationOperationLog
{
    public Guid Id { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? NotificationId { get; set; }
    public Guid? UserId { get; set; }
    public int AttemptNumber { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Metadata { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}