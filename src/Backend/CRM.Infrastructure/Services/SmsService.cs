using CRM.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for sending SMS messages
/// This is a basic implementation - in production you would integrate with a real SMS provider like Twilio, AWS SNS, etc.
/// </summary>
public class SmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsService> _logger;
    private readonly HttpClient _httpClient;

    public SmsService(
        IConfiguration configuration,
        ILogger<SmsService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<SmsResult> SendSmsAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending SMS to {PhoneNumber}", MaskPhoneNumber(phoneNumber));

            // In a real implementation, you would integrate with an SMS provider
            // For now, we'll simulate the SMS sending
            var result = await SimulateSmsDelivery(phoneNumber, message, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully sent SMS to {PhoneNumber} with message ID {MessageId}", 
                    MaskPhoneNumber(phoneNumber), result.MessageId);
            }
            else
            {
                _logger.LogWarning("Failed to send SMS to {PhoneNumber}: {Error}", 
                    MaskPhoneNumber(phoneNumber), result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", MaskPhoneNumber(phoneNumber));
            
            return new SmsResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message,
                ErrorDetails = ex.ToString()
            };
        }
    }

    public async Task<SmsDeliveryStatus> GetDeliveryStatusAsync(
        string messageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking delivery status for SMS message {MessageId}", messageId);

            // In a real implementation, you would query the SMS provider's API
            // For now, we'll simulate checking the status
            await Task.Delay(100, cancellationToken); // Simulate API call

            // Simulate that most messages are delivered successfully
            var random = new Random();
            var statusValue = random.Next(1, 101);

            return statusValue switch
            {
                <= 85 => SmsDeliveryStatus.Delivered,
                <= 95 => SmsDeliveryStatus.Pending,
                _ => SmsDeliveryStatus.Failed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking delivery status for SMS message {MessageId}", messageId);
            return SmsDeliveryStatus.Unknown;
        }
    }

    private async Task<SmsResult> SimulateSmsDelivery(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken)
    {
        // Simulate network delay
        await Task.Delay(Random.Shared.Next(100, 500), cancellationToken);

        // Simulate occasional failures (5% failure rate)
        var random = new Random();
        var shouldFail = random.Next(1, 101) <= 5;

        if (shouldFail)
        {
            return new SmsResult
            {
                IsSuccess = false,
                ErrorMessage = "SMS delivery failed - network error",
                ErrorDetails = "Simulated network failure"
            };
        }

        // Generate a fake message ID
        var messageId = $"sms_{Guid.NewGuid():N}";

        return new SmsResult
        {
            IsSuccess = true,
            MessageId = messageId
        };
    }

    private static string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
        {
            return "****";
        }

        // Show only the last 4 digits
        return "****" + phoneNumber.Substring(phoneNumber.Length - 4);
    }
}

/// <summary>
/// Configuration for SMS service providers
/// </summary>
public class SmsConfiguration
{
    public string Provider { get; set; } = "Simulation";
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
    public bool EnableDeliveryReports { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMinutes(1);
}