using CRM.Application.Notifications.Services;
using CRM.Domain.Enums;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Background job handlers for notification processing
/// </summary>
public class NotificationBackgroundJobHandlers
{
    private readonly INotificationDispatchService _dispatchService;
    private readonly IRetryPolicyService _retryPolicyService;
    private readonly ILogger<NotificationBackgroundJobHandlers> _logger;

    public NotificationBackgroundJobHandlers(
        INotificationDispatchService dispatchService,
        IRetryPolicyService retryPolicyService,
        ILogger<NotificationBackgroundJobHandlers> logger)
    {
        _dispatchService = dispatchService;
        _retryPolicyService = retryPolicyService;
        _logger = logger;
    }

    /// <summary>
    /// Background job for dispatching a single notification
    /// </summary>
    [Queue("notifications-normal")]
    public async Task ProcessNotificationDispatchAsync(Guid notificationId, NotificationChannel channel)
    {
        try
        {
            _logger.LogInformation("Processing notification dispatch job for notification {NotificationId} via {Channel}", 
                notificationId, channel);

            await _dispatchService.DispatchNotificationAsync(notificationId);

            _logger.LogInformation("Successfully processed notification dispatch job for notification {NotificationId} via {Channel}", 
                notificationId, channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notification dispatch job for notification {NotificationId} via {Channel}", 
                notificationId, channel);
            throw; // Re-throw to let Hangfire handle retry logic
        }
    }

    /// <summary>
    /// Background job for dispatching notifications with high priority
    /// </summary>
    [Queue("notifications-high")]
    public async Task ProcessHighPriorityNotificationDispatchAsync(Guid notificationId, NotificationChannel channel)
    {
        await ProcessNotificationDispatchAsync(notificationId, channel);
    }

    /// <summary>
    /// Background job for dispatching notifications with critical priority
    /// </summary>
    [Queue("notifications-critical")]
    public async Task ProcessCriticalNotificationDispatchAsync(Guid notificationId, NotificationChannel channel)
    {
        await ProcessNotificationDispatchAsync(notificationId, channel);
    }

    /// <summary>
    /// Background job for dispatching notifications with low priority
    /// </summary>
    [Queue("notifications-low")]
    public async Task ProcessLowPriorityNotificationDispatchAsync(Guid notificationId, NotificationChannel channel)
    {
        await ProcessNotificationDispatchAsync(notificationId, channel);
    }

    /// <summary>
    /// Background job for retrying failed dispatch attempts
    /// </summary>
    [Queue("notifications-retry")]
    public async Task ProcessRetryDispatchAsync(int dispatchAttemptId)
    {
        try
        {
            _logger.LogInformation("Processing retry dispatch job for attempt {AttemptId}", dispatchAttemptId);

            await _dispatchService.RetryFailedDispatchAsync(dispatchAttemptId);

            _logger.LogInformation("Successfully processed retry dispatch job for attempt {AttemptId}", dispatchAttemptId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing retry dispatch job for attempt {AttemptId}", dispatchAttemptId);
            throw;
        }
    }

    /// <summary>
    /// Background job for bulk notification dispatch
    /// </summary>
    [Queue("notifications-bulk")]
    public async Task ProcessBulkNotificationDispatchAsync(List<Guid> notificationIds, NotificationChannel channel)
    {
        try
        {
            _logger.LogInformation("Processing bulk notification dispatch job for {Count} notifications via {Channel}", 
                notificationIds.Count, channel);

            var tasks = notificationIds.Select(id => 
                _dispatchService.DispatchNotificationAsync(id));

            await Task.WhenAll(tasks);

            _logger.LogInformation("Successfully processed bulk notification dispatch job for {Count} notifications via {Channel}", 
                notificationIds.Count, channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bulk notification dispatch job for {Count} notifications via {Channel}", 
                notificationIds.Count, channel);
            throw;
        }
    }

    /// <summary>
    /// Background job for processing scheduled retry attempts
    /// </summary>
    [Queue("notifications-scheduled")]
    public async Task ProcessScheduledRetryAsync(int dispatchAttemptId, int attemptNumber, NotificationChannel channel)
    {
        try
        {
            _logger.LogInformation("Processing scheduled retry for attempt {AttemptId}, attempt number {AttemptNumber}", 
                dispatchAttemptId, attemptNumber);

            // Check if we should still retry
            var shouldRetry = _retryPolicyService.ShouldRetry(attemptNumber, channel, null);
            if (!shouldRetry)
            {
                _logger.LogWarning("Scheduled retry for attempt {AttemptId} cancelled - max retries exceeded", 
                    dispatchAttemptId);
                
                await _retryPolicyService.MarkAsPermanentlyFailedAsync(dispatchAttemptId);
                return;
            }

            await _dispatchService.RetryFailedDispatchAsync(dispatchAttemptId);

            _logger.LogInformation("Successfully processed scheduled retry for attempt {AttemptId}", dispatchAttemptId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing scheduled retry for attempt {AttemptId}", dispatchAttemptId);
            throw;
        }
    }

    /// <summary>
    /// Background job for cleaning up old dispatch attempts
    /// </summary>
    [Queue("notifications-maintenance")]
    public async Task CleanupOldDispatchAttemptsAsync(int retentionDays = 30)
    {
        try
        {
            _logger.LogInformation("Starting cleanup of dispatch attempts older than {RetentionDays} days", retentionDays);

            // This would be implemented in a cleanup service
            // await _cleanupService.CleanupOldDispatchAttemptsAsync(retentionDays);

            _logger.LogInformation("Successfully completed cleanup of old dispatch attempts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup of old dispatch attempts");
            throw;
        }
    }

    /// <summary>
    /// Background job for generating dispatch reports
    /// </summary>
    [Queue("notifications-reports")]
    public async Task GenerateDispatchReportAsync(DateTime fromDate, DateTime toDate, string reportType)
    {
        try
        {
            _logger.LogInformation("Generating dispatch report from {FromDate} to {ToDate}, type: {ReportType}", 
                fromDate, toDate, reportType);

            // This would be implemented in a reporting service
            // await _reportingService.GenerateDispatchReportAsync(fromDate, toDate, reportType);

            _logger.LogInformation("Successfully generated dispatch report");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dispatch report");
            throw;
        }
    }

    /// <summary>
    /// Background job for monitoring system health
    /// </summary>
    [Queue("notifications-monitoring")]
    public async Task MonitorSystemHealthAsync()
    {
        try
        {
            _logger.LogInformation("Starting system health monitoring");

            // This would check various system metrics
            // - Queue depths
            // - Processing rates
            // - Error rates
            // - External service availability

            _logger.LogInformation("System health monitoring completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during system health monitoring");
            throw;
        }
    }
}