using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using CRM.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Background service for processing notification retry attempts
/// </summary>
public class NotificationRetryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationRetryBackgroundService> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromMinutes(5);

    public NotificationRetryBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<NotificationRetryBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification retry background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingRetries(stoppingToken);
                await CleanupOldAttempts(stoppingToken);
                
                await Task.Delay(_processingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Notification retry background service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in notification retry background service");
                
                // Wait a bit before retrying to avoid tight error loops
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        _logger.LogInformation("Notification retry background service stopped");
    }

    private async Task ProcessPendingRetries(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var retryPolicyService = scope.ServiceProvider.GetRequiredService<IRetryPolicyService>();

        try
        {
            // Find failed attempts that are due for retry
            var failedAttempts = await context.NotificationDispatchAttempts
                .Where(da => da.Status == DispatchStatus.Failed && 
                           (da.NextRetryAt == null || da.NextRetryAt <= DateTime.UtcNow))
                .Select(da => new { da.Id, da.AttemptNumber, da.Channel, da.ErrorMessage })
                .ToListAsync(cancellationToken);

            if (failedAttempts.Any())
            {
                _logger.LogInformation("Processing {Count} failed dispatch attempts for retry", failedAttempts.Count);

                foreach (var attempt in failedAttempts)
                {
                    try
                    {
                        var shouldRetry = retryPolicyService.ShouldRetry(
                            attempt.AttemptNumber + 1, 
                            attempt.Channel, 
                            attempt.ErrorMessage);

                        if (shouldRetry)
                        {
                            await retryPolicyService.ScheduleRetryAsync(
                                attempt.Id,
                                attempt.AttemptNumber + 1,
                                attempt.Channel,
                                cancellationToken);
                        }
                        else
                        {
                            await retryPolicyService.MarkAsPermanentlyFailedAsync(attempt.Id, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing retry for dispatch attempt {AttemptId}", attempt.Id);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing pending retries");
        }
    }

    private async Task CleanupOldAttempts(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        try
        {
            // Clean up old dispatch attempts (older than 30 days)
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            
            var deleted = await context.NotificationDispatchAttempts
                .Where(da => da.AttemptedAt < cutoffDate && 
                           (da.Status == DispatchStatus.Delivered || da.Status == DispatchStatus.PermanentlyFailed))
                .ExecuteDeleteAsync(cancellationToken);

            if (deleted > 0)
            {
                _logger.LogInformation("Cleaned up {Count} old dispatch attempts", deleted);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old dispatch attempts");
        }
    }
}

/// <summary>
/// Hangfire job for processing individual retry attempts
/// </summary>
[Queue("notifications")]
public class NotificationRetryJob
{
    private readonly INotificationDispatchService _dispatchService;
    private readonly IRetryPolicyService _retryPolicyService;
    private readonly ILogger<NotificationRetryJob> _logger;

    public NotificationRetryJob(
        INotificationDispatchService dispatchService,
        IRetryPolicyService retryPolicyService,
        ILogger<NotificationRetryJob> logger)
    {
        _dispatchService = dispatchService;
        _retryPolicyService = retryPolicyService;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 0)] // We handle retries ourselves
    public async Task ProcessRetryAsync(int dispatchAttemptId)
    {
        try
        {
            _logger.LogInformation("Processing retry for dispatch attempt {AttemptId}", dispatchAttemptId);

            await _dispatchService.RetryFailedDispatchAsync(dispatchAttemptId);

            _logger.LogDebug("Successfully processed retry for dispatch attempt {AttemptId}", dispatchAttemptId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing retry for dispatch attempt {AttemptId}", dispatchAttemptId);
            
            // The retry policy service will handle scheduling the next retry or marking as permanently failed
            throw;
        }
    }
}

/// <summary>
/// Hangfire recurring job for monitoring retry statistics
/// </summary>
public class RetryMonitoringJob
{
    private readonly IRetryPolicyService _retryPolicyService;
    private readonly ILogger<RetryMonitoringJob> _logger;

    public RetryMonitoringJob(
        IRetryPolicyService retryPolicyService,
        ILogger<RetryMonitoringJob> logger)
    {
        _retryPolicyService = retryPolicyService;
        _logger = logger;
    }

    [Queue("monitoring")]
    public async Task GenerateRetryStatisticsAsync()
    {
        try
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-1); // Last 24 hours

            var statistics = await _retryPolicyService.GetRetryStatisticsAsync(startDate, endDate);

            _logger.LogInformation("Retry Statistics (Last 24h): Total: {Total}, Successful: {Successful}, Permanent Failures: {Failures}, Success Rate: {SuccessRate:P2}",
                statistics.TotalRetryAttempts,
                statistics.SuccessfulRetries,
                statistics.PermanentFailures,
                statistics.TotalRetryAttempts > 0 ? (double)statistics.SuccessfulRetries / statistics.TotalRetryAttempts : 0);

            // Log channel-specific statistics
            foreach (var channelStat in statistics.RetriesByChannel)
            {
                _logger.LogInformation("Channel {Channel}: {RetryCount} retries", channelStat.Key, channelStat.Value);
            }

            // Log failure reasons
            foreach (var failureReason in statistics.FailureReasons)
            {
                _logger.LogInformation("Failure Reason '{Reason}': {Count} occurrences", failureReason.Key, failureReason.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating retry statistics");
        }
    }
}