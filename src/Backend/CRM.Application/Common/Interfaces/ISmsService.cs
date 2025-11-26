namespace CRM.Application.Common.Interfaces;

/// <summary>
/// Service for sending SMS messages
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Sends an SMS message to the specified phone number
    /// </summary>
    Task<SmsResult> SendSmsAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the delivery status of an SMS message
    /// </summary>
    Task<SmsDeliveryStatus> GetDeliveryStatusAsync(
        string messageId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of SMS sending operation
/// </summary>
public class SmsResult
{
    public bool IsSuccess { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorDetails { get; set; }
}

/// <summary>
/// SMS delivery status
/// </summary>
public enum SmsDeliveryStatus
{
    Pending,
    Delivered,
    Failed,
    Unknown
}