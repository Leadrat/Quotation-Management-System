using CRM.Application.Common.Interfaces;
using CRM.Application.Notifications.Services;
using CRM.Domain.Enums;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing notification dispatch queues with priority and throttling
/// </summary>
public class NotificationQueueService : INotificationQueueService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<NotificationQueueService> _logger;
    private readonly NotificationQueueConfiguration _configuration;

    public NotificationQueueService(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager,
        IConfiguration configuration,
        ILogger<NotificationQueueService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _logger = logger;
        _configuration = LoadConfiguration(configuration);
    }

    public async Task<string> EnqueueNotificationDispatchAsync(
        Guid notificationId,
        NotificationChannel channel,
        NotificationPriority priority = NotificationPriority.Normal,
        TimeSpan? delay = null)
    {
        try
        {
            var queueName = GetQueueNameForPriority(priority);
            
            _logger.LogInformation("Enqueueing notification {NotificationId} for {Channel} dispatch with {Priority} priority",
                notificationId, channel, priority);

            string jobId;
            
            if (delay.HasValue && delay.Value > TimeSpan.Zero)
            {
                // Schedule for later dispatch
                jobId = _backgroundJobClient.Schedule<INotificationDispatchService>(
                    service => service.DispatchNotificationAsync(notificationId),
                    delay.Value);
            }
            else
            {
                // Immediate dispatch
                jobId = _backgroundJobClient.Enqueue<INotificationDispatchService>(
                    queueName,
                    service => service.DispatchNotificationAsync(notificationId));
            }

            _logger.LogDebug("Notification {NotificationId} enqueued with job ID {JobId} in queue {Queue}",
                notificationId, jobId, queueName);

            return jobId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enqueueing notification {NotificationId} for {Channel} dispatch",
                notificationId, channel);
            throw;
        }
    }

    public async Task<List<string>> EnqueueBulkNotificationDispatchAsync(
        List<Guid> notificationIds,
        NotificationChannel channel,
        NotificationPriority priority = NotificationPriority.Normal)
    {
        try
        {
            _logger.LogInformation("Enqueueing {Count} notifications for {Channel} dispatch with {Priority} priority",
                notificationIds.Count, channel, priority);

            var jobIds = new List<string>();
            var queueName = GetQueueNameForPriority(priority);

            // Apply throttling for bulk operations
            var batchSize = GetBatchSizeForPriority(priority);
            var batches = notificationIds.Chunk(batchSize);

            foreach (var batch in batches)
            {
                foreach (var notificationId in batch)
                {
                    var jobId = _backgroundJobClient.Enqueue<INotificationDispatchService>(
                        queueName,
                        service => service.DispatchNotificationAsync(notificationId));
                    
                    jobIds.Add(jobId);
                }

                // Add delay between batches to prevent overwhelming the system
                if (batches.Count() > 1)
                {
                    await Task.Delay(_configuration.BatchDelayMs);
                }
            }

            _logger.LogInformation("Successfully enqueued {Count} notifications in {BatchCount} batches",
                notificationIds.Count, batches.Count());

            return jobIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enqueueing bulk notifications for {Channel} dispatch", channel);
            throw;
        }
    }

    public async Task<QueueStatistics> GetQueueStatisticsAsync()
    {
        try
        {
            var monitoring = JobStorage.Current.GetMonitoringApi();
            
            var statistics = new QueueStatistics
            {
                Timestamp = DateTime.UtcNow,
                QueueDepths = new Dictionary<string, int>(),
                ProcessingCounts = new Dictionary<string, int>(),
                FailedCounts = new Dictionary<string, int>()
            };

            // Get statistics for each priority queue
            foreach (var queueName in _configuration.QueueNames.Values)
            {
                var queueJobs = monitoring.Queues().FirstOrDefault(q => q.Name == queueName);
                if (queueJobs != null)
                {
                    statistics.QueueDepths[queueName] = (int)queueJobs.Length;
                }

                var processingJobs = monitoring.ProcessingJobs(0, int.MaxValue);
                statistics.ProcessingCounts[queueName] = processingJobs.Count();

                var failedJobs = monitoring.FailedJobs(0, int.MaxValue);
                statistics.FailedCounts[queueName] = failedJobs.Count();
            }

            // Calculate totals
            statistics.TotalEnqueued = statistics.QueueDepths.Values.Sum();
            statistics.TotalProcessing = statistics.ProcessingCounts.Values.Sum();
            statistics.TotalFailed = statistics.FailedCounts.Values.Sum();

            _logger.LogDebug("Queue statistics: {TotalEnqueued} enqueued, {TotalProcessing} processing, {TotalFailed} failed",
                statistics.TotalEnqueued, statistics.TotalProcessing, statistics.TotalFailed);

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving queue statistics");
            throw;
        }
    }

    public async Task SetupRecurringMonitoringJobsAsync()
    {
        try
        {
            _logger.LogInformation("Setting up recurring monitoring jobs");

            // Queue depth monitoring
            _recurringJobManager.AddOrUpdate(
                "queue-depth-monitoring",
                () => MonitorQueueDepthAsync(),
                _configuration.MonitoringCron);

            // Performance metrics collection
            _recurringJobManager.AddOrUpdate(
                "performance-metrics-collection",
                () => CollectPerformanceMetricsAsync(),
                _configuration.MetricsCron);

            // Failed job cleanup
            _recurringJobManager.AddOrUpdate(
                "failed-job-cleanup",
                () => CleanupFailedJobsAsync(),
                _configuration.CleanupCron);

            _logger.LogInformation("Recurring monitoring jobs configured successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up recurring monitoring jobs");
            throw;
        }
    }

    public async Task MonitorQueueDepthAsync()
    {
        try
        {
            var statistics = await GetQueueStatisticsAsync();
            
            // Check for queue depth alerts
            foreach (var queueDepth in statistics.QueueDepths)
            {
                var threshold = GetQueueDepthThreshold(queueDepth.Key);
                if (queueDepth.Value > threshold)
                {
                    _logger.LogWarning("Queue {QueueName} depth ({Depth}) exceeds threshold ({Threshold})",
                        queueDepth.Key, queueDepth.Value, threshold);
                    
                    // Could trigger alerts here
                    await TriggerQueueDepthAlertAsync(queueDepth.Key, queueDepth.Value, threshold);
                }
            }

            // Log overall statistics
            _logger.LogInformation("Queue monitoring: Total enqueued: {TotalEnqueued}, Processing: {TotalProcessing}, Failed: {TotalFailed}",
                statistics.TotalEnqueued, statistics.TotalProcessing, statistics.TotalFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring queue depth");
        }
    }

    private async Task CollectPerformanceMetricsAsync()
    {
        try
        {
            var statistics = await GetQueueStatisticsAsync();
            
            // Store metrics for analysis (could integrate with metrics system like Prometheus)
            _logger.LogInformation("Performance metrics collected: {Metrics}", 
                System.Text.Json.JsonSerializer.Serialize(statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting performance metrics");
        }
    }

    private async Task CleanupFailedJobsAsync()
    {
        try
        {
            var monitoring = JobStorage.Current.GetMonitoringApi();
            var cutoffDate = DateTime.UtcNow.AddDays(-_configuration.FailedJobRetentionDays);
            
            var failedJobs = monitoring.FailedJobs(0, int.MaxValue)
                .Where(job => job.Value.FailedAt < cutoffDate)
                .ToList();

            if (failedJobs.Any())
            {
                _logger.LogInformation("Cleaning up {Count} old failed jobs", failedJobs.Count);
                
                foreach (var failedJob in failedJobs)
                {
                    BackgroundJob.Delete(failedJob.Key);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up failed jobs");
        }
    }

    private async Task TriggerQueueDepthAlertAsync(string queueName, int currentDepth, int threshold)
    {
        // Implementation for alerting system administrators
        _logger.LogCritical("ALERT: Queue {QueueName} depth ({CurrentDepth}) exceeds threshold ({Threshold})",
            queueName, currentDepth, threshold);
        
        // Could send email, Slack notification, etc.
    }

    private string GetQueueNameForPriority(NotificationPriority priority)
    {
        return _configuration.QueueNames.TryGetValue(priority, out var queueName) 
            ? queueName 
            : _configuration.QueueNames[NotificationPriority.Normal];
    }

    private int GetBatchSizeForPriority(NotificationPriority priority)
    {
        return priority switch
        {
            NotificationPriority.Critical => _configuration.CriticalBatchSize,
            NotificationPriority.High => _configuration.HighBatchSize,
            NotificationPriority.Normal => _configuration.NormalBatchSize,
            NotificationPriority.Low => _configuration.LowBatchSize,
            _ => _configuration.NormalBatchSize
        };
    }

    private int GetQueueDepthThreshold(string queueName)
    {
        return _configuration.QueueDepthThresholds.TryGetValue(queueName, out var threshold) 
            ? threshold 
            : _configuration.DefaultQueueDepthThreshold;
    }

    private static NotificationQueueConfiguration LoadConfiguration(IConfiguration configuration)
    {
        var config = new NotificationQueueConfiguration();
        configuration.GetSection("NotificationQueue").Bind(config);
        return config;
    }
}

/// <summary>
/// Interface for notification queue management
/// </summary>
public interface INotificationQueueService
{
    Task<string> EnqueueNotificationDispatchAsync(
        Guid notificationId,
        NotificationChannel channel,
        NotificationPriority priority = NotificationPriority.Normal,
        TimeSpan? delay = null);

    Task<List<string>> EnqueueBulkNotificationDispatchAsync(
        List<Guid> notificationIds,
        NotificationChannel channel,
        NotificationPriority priority = NotificationPriority.Normal);

    Task<QueueStatistics> GetQueueStatisticsAsync();
    Task SetupRecurringMonitoringJobsAsync();
    Task MonitorQueueDepthAsync();
}

/// <summary>
/// Configuration for notification queue processing
/// </summary>
public class NotificationQueueConfiguration
{
    public Dictionary<NotificationPriority, string> QueueNames { get; set; } = new()
    {
        { NotificationPriority.Critical, "notifications-critical" },
        { NotificationPriority.High, "notifications-high" },
        { NotificationPriority.Normal, "notifications-normal" },
        { NotificationPriority.Low, "notifications-low" }
    };

    public Dictionary<string, int> QueueDepthThresholds { get; set; } = new()
    {
        { "notifications-critical", 10 },
        { "notifications-high", 50 },
        { "notifications-normal", 200 },
        { "notifications-low", 500 }
    };

    public int DefaultQueueDepthThreshold { get; set; } = 100;
    public int CriticalBatchSize { get; set; } = 5;
    public int HighBatchSize { get; set; } = 10;
    public int NormalBatchSize { get; set; } = 25;
    public int LowBatchSize { get; set; } = 50;
    public int BatchDelayMs { get; set; } = 100;
    public int FailedJobRetentionDays { get; set; } = 7;
    
    public string MonitoringCron { get; set; } = "*/5 * * * *"; // Every 5 minutes
    public string MetricsCron { get; set; } = "0 */1 * * *"; // Every hour
    public string CleanupCron { get; set; } = "0 2 * * *"; // Daily at 2 AM
}

/// <summary>
/// Queue statistics for monitoring
/// </summary>
public class QueueStatistics
{
    public DateTime Timestamp { get; set; }
    public Dictionary<string, int> QueueDepths { get; set; } = new();
    public Dictionary<string, int> ProcessingCounts { get; set; } = new();
    public Dictionary<string, int> FailedCounts { get; set; } = new();
    public int TotalEnqueued { get; set; }
    public int TotalProcessing { get; set; }
    public int TotalFailed { get; set; }
}