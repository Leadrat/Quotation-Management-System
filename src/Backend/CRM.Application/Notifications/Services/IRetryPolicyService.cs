using CRM.Domain.Enums;

namespace CRM.Application.Notifications.Services;

/// <summary>
/// Service for managing retry policies and scheduling retry attempts
/// </summary>
public interface IRetryPolicyService
{
    /// <summary>
    /// Calculates the next retry delay using exponential backoff
    /// </summary>
    TimeSpan CalculateRetryDelay(int attemptNumber, NotificationChannel channel);

    /// <summary>
    /// Determines if a failed dispatch should be retried
    /// </summary>
    bool ShouldRetry(int attemptNumber, NotificationChannel channel, string? errorMessage = null);

    /// <summary>
    /// Gets the maximum number of retry attempts for a channel
    /// </summary>
    int GetMaxRetryAttempts(NotificationChannel channel);

    /// <summary>
    /// Schedules a retry attempt for a failed dispatch
    /// </summary>
    Task ScheduleRetryAsync(
        int dispatchAttemptId,
        int attemptNumber,
        NotificationChannel channel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a dispatch as permanently failed after max retries
    /// </summary>
    Task MarkAsPermanentlyFailedAsync(
        int dispatchAttemptId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets retry statistics for monitoring
    /// </summary>
    Task<RetryStatistics> GetRetryStatisticsAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Configuration for retry policies
/// </summary>
public class RetryPolicyConfiguration
{
    public NotificationChannel Channel { get; set; }
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromMinutes(1);
    public double BackoffMultiplier { get; set; } = 2.0;
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromHours(1);
    public List<string> NonRetryableErrors { get; set; } = new();
}

/// <summary>
/// Statistics about retry attempts
/// </summary>
public class RetryStatistics
{
    public int TotalRetryAttempts { get; set; }
    public int SuccessfulRetries { get; set; }
    public int PermanentFailures { get; set; }
    public Dictionary<NotificationChannel, int> RetriesByChannel { get; set; } = new();
    public Dictionary<string, int> FailureReasons { get; set; } = new();
    public double AverageRetryDelay { get; set; }
}