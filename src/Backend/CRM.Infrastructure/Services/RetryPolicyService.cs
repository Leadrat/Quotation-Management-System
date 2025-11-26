using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing retry policies and scheduling retry attempts with exponential backoff
/// </summary>
public class RetryPolicyService : IRetryPolicyService
{
    private readonly IAppDbContext _context;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<RetryPolicyService> _logger;
    private readonly Dictionary<NotificationChannel, RetryPolicyConfiguration> _retryPolicies;

    public RetryPolicyService(
        IAppDbContext context,
        IBackgroundJobClient backgroundJobClient,
        IConfiguration configuration,
        ILogger<RetryPolicyService> logger)
    {
        _context = context;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
        _retryPolicies = LoadRetryPolicies(configuration);
    }

    public TimeSpan CalculateRetryDelay(int attemptNumber, NotificationChannel channel)
    {
        var policy = GetRetryPolicy(channel);
        
        // Exponential backoff: baseDelay * (multiplier ^ (attemptNumber - 1))
        var delay = TimeSpan.FromMilliseconds(
            policy.BaseDelay.TotalMilliseconds * Math.Pow(policy.BackoffMultiplier, attemptNumber - 1));

        // Add jitter to prevent thundering herd (±25% random variation)
        var jitter = Random.Shared.NextDouble() * 0.5 - 0.25; // -0.25 to +0.25
        delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * (1 + jitter));

        // Cap at maximum delay
        if (delay > policy.MaxDelay)
        {
            delay = policy.MaxDelay;
        }

        _logger.LogDebug("Calculated retry delay for attempt {AttemptNumber} on {Channel}: {Delay}", 
            attemptNumber, channel, delay);

        return delay;
    }

    public bool ShouldRetry(int attemptNumber, NotificationChannel channel, string? errorMessage = null)
    {
        var policy = GetRetryPolicy(channel);

        // Check if we've exceeded max attempts
        if (attemptNumber >= policy.MaxRetryAttempts)
        {
            _logger.LogDebug("Max retry attempts ({MaxAttempts}) reached for {Channel}", 
                policy.MaxRetryAttempts, channel);
            return false;
        }

        // Check if error is non-retryable
        if (!string.IsNullOrEmpty(errorMessage) && 
            policy.NonRetryableErrors.Any(error => errorMessage.Contains(error, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogDebug("Non-retryable error detected for {Channel}: {Error}", channel, errorMessage);
            return false;
        }

        return true;
    }

    public int GetMaxRetryAttempts(NotificationChannel channel)
    {
        return GetRetryPolicy(channel).MaxRetryAttempts;
    }

    public async Task ScheduleRetryAsync(
        int dispatchAttemptId,
        int attemptNumber,
        NotificationChannel channel,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var delay = CalculateRetryDelay(attemptNumber, channel);
            var scheduledTime = DateTime.UtcNow.Add(delay);

            _logger.LogInformation("Scheduling retry for dispatch attempt {AttemptId} in {Delay} (at {ScheduledTime})", 
                dispatchAttemptId, delay, scheduledTime);

            // Schedule the retry using Hangfire
            _backgroundJobClient.Schedule<INotificationDispatchService>(
                service => service.RetryFailedDispatchAsync(dispatchAttemptId),
                delay);

            // Update the dispatch attempt to indicate retry is scheduled
            var dispatchAttempt = await _context.NotificationDispatchAttempts
                .FirstOrDefaultAsync(da => da.Id == dispatchAttemptId, cancellationToken);

            if (dispatchAttempt != null)
            {
                dispatchAttempt.NextRetryAt = scheduledTime;
                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogDebug("Successfully scheduled retry for dispatch attempt {AttemptId}", dispatchAttemptId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling retry for dispatch attempt {AttemptId}", dispatchAttemptId);
            throw;
        }
    }

    public async Task MarkAsPermanentlyFailedAsync(
        int dispatchAttemptId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Marking dispatch attempt {AttemptId} as permanently failed", dispatchAttemptId);

            var dispatchAttempt = await _context.NotificationDispatchAttempts
                .FirstOrDefaultAsync(da => da.Id == dispatchAttemptId, cancellationToken);

            if (dispatchAttempt != null)
            {
                dispatchAttempt.Status = DispatchStatus.PermanentlyFailed;
                dispatchAttempt.ErrorMessage = (dispatchAttempt.ErrorMessage ?? "") + " [PERMANENTLY FAILED - MAX RETRIES EXCEEDED]";
                dispatchAttempt.NextRetryAt = null;
                
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogWarning("Dispatch attempt {AttemptId} marked as permanently failed after {AttemptNumber} attempts", 
                    dispatchAttemptId, dispatchAttempt.AttemptNumber);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking dispatch attempt {AttemptId} as permanently failed", dispatchAttemptId);
            throw;
        }
    }

    public async Task<RetryStatistics> GetRetryStatisticsAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var attempts = await _context.NotificationDispatchAttempts
                .Where(da => da.AttemptedAt >= fromDate && da.AttemptedAt <= toDate)
                .ToListAsync(cancellationToken);

            var retryAttempts = attempts.Where(da => da.AttemptNumber > 1).ToList();
            var successfulRetries = retryAttempts.Where(da => da.Status == DispatchStatus.Delivered).ToList();
            var permanentFailures = attempts.Where(da => da.Status == DispatchStatus.PermanentlyFailed).ToList();

            var statistics = new RetryStatistics
            {
                TotalRetryAttempts = retryAttempts.Count,
                SuccessfulRetries = successfulRetries.Count,
                PermanentFailures = permanentFailures.Count,
                RetriesByChannel = retryAttempts
                    .GroupBy(da => da.Channel)
                    .ToDictionary(g => g.Key, g => g.Count()),
                FailureReasons = permanentFailures
                    .Where(da => !string.IsNullOrEmpty(da.ErrorMessage))
                    .GroupBy(da => ExtractFailureReason(da.ErrorMessage!))
                    .ToDictionary(g => g.Key, g => g.Count()),
                AverageRetryDelay = retryAttempts.Any() 
                    ? retryAttempts.Average(da => (da.AttemptedAt - da.Notification.CreatedAt).TotalMinutes)
                    : 0
            };

            _logger.LogDebug("Generated retry statistics for period {FromDate} to {ToDate}: {TotalRetries} total retries, {SuccessfulRetries} successful", 
                fromDate, toDate, statistics.TotalRetryAttempts, statistics.SuccessfulRetries);

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating retry statistics for period {FromDate} to {ToDate}", fromDate, toDate);
            throw;
        }
    }

    private RetryPolicyConfiguration GetRetryPolicy(NotificationChannel channel)
    {
        return _retryPolicies.TryGetValue(channel, out var policy) 
            ? policy 
            : _retryPolicies[NotificationChannel.InApp]; // Default policy
    }

    private static Dictionary<NotificationChannel, RetryPolicyConfiguration> LoadRetryPolicies(IConfiguration configuration)
    {
        var policies = new Dictionary<NotificationChannel, RetryPolicyConfiguration>();

        // Default policies
        policies[NotificationChannel.InApp] = new RetryPolicyConfiguration
        {
            Channel = NotificationChannel.InApp,
            MaxRetryAttempts = 3,
            BaseDelay = TimeSpan.FromSeconds(30),
            BackoffMultiplier = 2.0,
            MaxDelay = TimeSpan.FromMinutes(10),
            NonRetryableErrors = new List<string> { "user not found", "invalid user" }
        };

        policies[NotificationChannel.Email] = new RetryPolicyConfiguration
        {
            Channel = NotificationChannel.Email,
            MaxRetryAttempts = 5,
            BaseDelay = TimeSpan.FromMinutes(1),
            BackoffMultiplier = 2.0,
            MaxDelay = TimeSpan.FromHours(1),
            NonRetryableErrors = new List<string> { "invalid email", "blocked recipient", "permanent failure" }
        };

        policies[NotificationChannel.SMS] = new RetryPolicyConfiguration
        {
            Channel = NotificationChannel.SMS,
            MaxRetryAttempts = 3,
            BaseDelay = TimeSpan.FromMinutes(2),
            BackoffMultiplier = 1.5,
            MaxDelay = TimeSpan.FromMinutes(30),
            NonRetryableErrors = new List<string> { "invalid phone", "blocked number", "carrier rejected" }
        };

        // Override with configuration values if present
        var retrySection = configuration.GetSection("NotificationRetryPolicies");
        if (retrySection.Exists())
        {
            foreach (var channelSection in retrySection.GetChildren())
            {
                if (Enum.TryParse<NotificationChannel>(channelSection.Key, out var channel))
                {
                    var policy = policies[channel];
                    
                    if (int.TryParse(channelSection["MaxRetryAttempts"], out var maxAttempts))
                        policy.MaxRetryAttempts = maxAttempts;
                    
                    if (TimeSpan.TryParse(channelSection["BaseDelay"], out var baseDelay))
                        policy.BaseDelay = baseDelay;
                    
                    if (double.TryParse(channelSection["BackoffMultiplier"], out var multiplier))
                        policy.BackoffMultiplier = multiplier;
                    
                    if (TimeSpan.TryParse(channelSection["MaxDelay"], out var maxDelay))
                        policy.MaxDelay = maxDelay;
                }
            }
        }

        return policies;
    }

    private static string ExtractFailureReason(string errorMessage)
    {
        // Extract the primary failure reason from error message
        var lowerError = errorMessage.ToLowerInvariant();
        
        if (lowerError.Contains("timeout")) return "Timeout";
        if (lowerError.Contains("network")) return "Network Error";
        if (lowerError.Contains("invalid")) return "Invalid Configuration";
        if (lowerError.Contains("blocked")) return "Blocked Recipient";
        if (lowerError.Contains("quota") || lowerError.Contains("limit")) return "Rate Limited";
        if (lowerError.Contains("authentication") || lowerError.Contains("unauthorized")) return "Authentication Error";
        
        return "Other";
    }
}