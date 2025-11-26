using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for comprehensive notification monitoring and performance tracking
/// </summary>
public class NotificationMonitoringService : INotificationMonitoringService
{
    private readonly IAppDbContext _context;
    private readonly ILogger<NotificationMonitoringService> _logger;
    private readonly Dictionary<string, PerformanceCounter> _performanceCounters;
    private readonly object _counterLock = new();

    public NotificationMonitoringService(
        IAppDbContext context,
        ILogger<NotificationMonitoringService> logger)
    {
        _context = context;
        _logger = logger;
        _performanceCounters = new Dictionary<string, PerformanceCounter>();
    }

    public async Task LogNotificationOperationAsync(
        NotificationOperation operation,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var activity = new Activity("NotificationOperation").Start();
            activity?.SetTag("operation.type", operation.OperationType.ToString());
            activity?.SetTag("operation.channel", operation.Channel.ToString());
            activity?.SetTag("operation.status", operation.Status.ToString());

            // Structured logging with all operation details
            _logger.LogInformation(
                "Notification Operation: {OperationType} | Channel: {Channel} | Status: {Status} | " +
                "Duration: {Duration}ms | NotificationId: {NotificationId} | UserId: {UserId} | " +
                "AttemptNumber: {AttemptNumber} | ErrorMessage: {ErrorMessage}",
                operation.OperationType,
                operation.Channel,
                operation.Status,
                operation.Duration?.TotalMilliseconds,
                operation.NotificationId,
                operation.UserId,
                operation.AttemptNumber,
                operation.ErrorMessage);

            // Update performance counters
            UpdatePerformanceCounters(operation);

            // Store operation log in database for analysis
            var operationLog = new NotificationOperationLog
            {
                Id = Guid.NewGuid(),
                OperationType = operation.OperationType.ToString(),
                Channel = operation.Channel.ToString(),
                Status = operation.Status.ToString(),
                NotificationId = operation.NotificationId,
                UserId = operation.UserId,
                AttemptNumber = operation.AttemptNumber,
                Duration = operation.Duration,
                ErrorMessage = operation.ErrorMessage,
                Metadata = operation.Metadata != null ? JsonSerializer.Serialize(operation.Metadata) : null,
                Timestamp = DateTimeOffset.UtcNow
            };

            _context.NotificationOperationLogs.Add(operationLog);
            await _context.SaveChangesAsync(cancellationToken);

            // Check for critical failures and trigger alerts
            if (operation.Status == OperationStatus.Failed && operation.AttemptNumber >= 3)
            {
                await TriggerCriticalFailureAlertAsync(operation, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging notification operation");
        }
    }

    public async Task<PerformanceMetrics> GetPerformanceMetricsAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var logs = await _context.NotificationOperationLogs
                .Where(log => log.Timestamp >= fromDate && log.Timestamp <= toDate)
                .ToListAsync(cancellationToken);

            var metrics = new PerformanceMetrics
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalOperations = logs.Count,
                SuccessfulOperations = logs.Count(l => l.Status == "Success"),
                FailedOperations = logs.Count(l => l.Status == "Failed"),
                AverageProcessingTime = logs.Where(l => l.Duration.HasValue)
                    .Average(l => l.Duration!.Value.TotalMilliseconds),
                OperationsByChannel = logs.GroupBy(l => l.Channel)
                    .ToDictionary(g => g.Key, g => g.Count()),
                OperationsByType = logs.GroupBy(l => l.OperationType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ErrorsByType = logs.Where(l => l.Status == "Failed" && !string.IsNullOrEmpty(l.ErrorMessage))
                    .GroupBy(l => ExtractErrorType(l.ErrorMessage!))
                    .ToDictionary(g => g.Key, g => g.Count()),
                ProcessingTimePercentiles = CalculatePercentiles(
                    logs.Where(l => l.Duration.HasValue)
                        .Select(l => l.Duration!.Value.TotalMilliseconds)
                        .ToList())
            };

            // Calculate success rate
            metrics.SuccessRate = metrics.TotalOperations > 0 
                ? (double)metrics.SuccessfulOperations / metrics.TotalOperations * 100 
                : 0;

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating performance metrics");
            throw;
        }
    }

    public async Task<List<AlertCondition>> CheckAlertConditionsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var alerts = new List<AlertCondition>();
            var now = DateTime.UtcNow;
            var oneHourAgo = now.AddHours(-1);

            // Get recent operations for analysis
            var recentLogs = await _context.NotificationOperationLogs
                .Where(log => log.Timestamp >= oneHourAgo)
                .ToListAsync(cancellationToken);

            // Check failure rate
            var totalOperations = recentLogs.Count;
            var failedOperations = recentLogs.Count(l => l.Status == "Failed");
            
            if (totalOperations > 0)
            {
                var failureRate = (double)failedOperations / totalOperations * 100;
                if (failureRate > 10) // Alert if failure rate > 10%
                {
                    alerts.Add(new AlertCondition
                    {
                        Type = AlertType.HighFailureRate,
                        Severity = failureRate > 25 ? AlertSeverity.Critical : AlertSeverity.Warning,
                        Message = $"High failure rate detected: {failureRate:F1}% ({failedOperations}/{totalOperations})",
                        Timestamp = now,
                        Metadata = new Dictionary<string, object>
                        {
                            ["FailureRate"] = failureRate,
                            ["FailedOperations"] = failedOperations,
                            ["TotalOperations"] = totalOperations
                        }
                    });
                }
            }

            // Check processing time
            var processingTimes = recentLogs.Where(l => l.Duration.HasValue)
                .Select(l => l.Duration!.Value.TotalMilliseconds)
                .ToList();

            if (processingTimes.Any())
            {
                var averageProcessingTime = processingTimes.Average();
                if (averageProcessingTime > 5000) // Alert if average > 5 seconds
                {
                    alerts.Add(new AlertCondition
                    {
                        Type = AlertType.SlowProcessing,
                        Severity = averageProcessingTime > 10000 ? AlertSeverity.Critical : AlertSeverity.Warning,
                        Message = $"Slow processing detected: {averageProcessingTime:F0}ms average",
                        Timestamp = now,
                        Metadata = new Dictionary<string, object>
                        {
                            ["AverageProcessingTime"] = averageProcessingTime,
                            ["SampleSize"] = processingTimes.Count
                        }
                    });
                }
            }

            // Check queue depth (would integrate with queue monitoring)
            lock (_counterLock)
            {
                foreach (var counter in _performanceCounters)
                {
                    if (counter.Key.Contains("QueueDepth") && counter.Value.Value > 1000)
                    {
                        alerts.Add(new AlertCondition
                        {
                            Type = AlertType.HighQueueDepth,
                            Severity = counter.Value.Value > 5000 ? AlertSeverity.Critical : AlertSeverity.Warning,
                            Message = $"High queue depth: {counter.Value.Value} items in {counter.Key}",
                            Timestamp = now,
                            Metadata = new Dictionary<string, object>
                            {
                                ["QueueName"] = counter.Key,
                                ["QueueDepth"] = counter.Value.Value
                            }
                        });
                    }
                }
            }

            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking alert conditions");
            throw;
        }
    }

    public async Task TriggerAlertAsync(
        AlertCondition alert,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogWarning("ALERT TRIGGERED: {AlertType} | Severity: {Severity} | Message: {Message}",
                alert.Type, alert.Severity, alert.Message);

            // Store alert in database
            var alertLog = new NotificationAlert
            {
                Id = Guid.NewGuid(),
                AlertType = alert.Type.ToString(),
                Severity = alert.Severity.ToString(),
                Message = alert.Message,
                Metadata = alert.Metadata != null ? JsonSerializer.Serialize(alert.Metadata) : null,
                Timestamp = alert.Timestamp,
                IsResolved = false
            };

            _context.NotificationAlerts.Add(alertLog);
            await _context.SaveChangesAsync(cancellationToken);

            // Send alert notifications (email, Slack, etc.)
            await SendAlertNotificationAsync(alert, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering alert");
        }
    }

    public void UpdatePerformanceCounter(string counterName, double value)
    {
        lock (_counterLock)
        {
            if (_performanceCounters.TryGetValue(counterName, out var counter))
            {
                counter.Value = value;
                counter.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                _performanceCounters[counterName] = new PerformanceCounter
                {
                    Name = counterName,
                    Value = value,
                    LastUpdated = DateTime.UtcNow
                };
            }
        }
    }

    public Dictionary<string, double> GetCurrentPerformanceCounters()
    {
        lock (_counterLock)
        {
            return _performanceCounters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);
        }
    }

    private void UpdatePerformanceCounters(NotificationOperation operation)
    {
        var channelKey = $"{operation.Channel}_Operations";
        var statusKey = $"{operation.Channel}_{operation.Status}_Operations";
        
        lock (_counterLock)
        {
            // Increment operation counters
            IncrementCounter(channelKey);
            IncrementCounter(statusKey);
            IncrementCounter("Total_Operations");

            // Update processing time
            if (operation.Duration.HasValue)
            {
                var processingTimeKey = $"{operation.Channel}_ProcessingTime_Ms";
                UpdatePerformanceCounter(processingTimeKey, operation.Duration.Value.TotalMilliseconds);
            }
        }
    }

    private void IncrementCounter(string counterName)
    {
        if (_performanceCounters.TryGetValue(counterName, out var counter))
        {
            counter.Value++;
            counter.LastUpdated = DateTime.UtcNow;
        }
        else
        {
            _performanceCounters[counterName] = new PerformanceCounter
            {
                Name = counterName,
                Value = 1,
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    private async Task TriggerCriticalFailureAlertAsync(
        NotificationOperation operation,
        CancellationToken cancellationToken)
    {
        var alert = new AlertCondition
        {
            Type = AlertType.CriticalFailure,
            Severity = AlertSeverity.Critical,
            Message = $"Critical failure after {operation.AttemptNumber} attempts: {operation.ErrorMessage}",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["NotificationId"] = operation.NotificationId ?? Guid.Empty,
                ["Channel"] = operation.Channel.ToString(),
                ["AttemptNumber"] = operation.AttemptNumber,
                ["ErrorMessage"] = operation.ErrorMessage ?? ""
            }
        };

        await TriggerAlertAsync(alert, cancellationToken);
    }

    private async Task SendAlertNotificationAsync(
        AlertCondition alert,
        CancellationToken cancellationToken)
    {
        // Implementation would send alerts via email, Slack, etc.
        // For now, just log the alert
        _logger.LogCritical("CRITICAL ALERT: {Message}", alert.Message);
    }

    private static string ExtractErrorType(string errorMessage)
    {
        var lowerError = errorMessage.ToLowerInvariant();
        
        if (lowerError.Contains("timeout")) return "Timeout";
        if (lowerError.Contains("network")) return "Network";
        if (lowerError.Contains("authentication")) return "Authentication";
        if (lowerError.Contains("invalid")) return "Validation";
        if (lowerError.Contains("quota") || lowerError.Contains("limit")) return "RateLimit";
        
        return "Other";
    }

    private static Dictionary<string, double> CalculatePercentiles(List<double> values)
    {
        if (!values.Any()) return new Dictionary<string, double>();

        values.Sort();
        var count = values.Count;

        return new Dictionary<string, double>
        {
            ["P50"] = GetPercentile(values, 0.5),
            ["P90"] = GetPercentile(values, 0.9),
            ["P95"] = GetPercentile(values, 0.95),
            ["P99"] = GetPercentile(values, 0.99)
        };
    }

    private static double GetPercentile(List<double> sortedValues, double percentile)
    {
        var index = (int)Math.Ceiling(sortedValues.Count * percentile) - 1;
        return sortedValues[Math.Max(0, Math.Min(index, sortedValues.Count - 1))];
    }
}

/// <summary>
/// Interface for notification monitoring service
/// </summary>
public interface INotificationMonitoringService
{
    Task LogNotificationOperationAsync(NotificationOperation operation, CancellationToken cancellationToken = default);
    Task<PerformanceMetrics> GetPerformanceMetricsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<List<AlertCondition>> CheckAlertConditionsAsync(CancellationToken cancellationToken = default);
    Task TriggerAlertAsync(AlertCondition alert, CancellationToken cancellationToken = default);
    void UpdatePerformanceCounter(string counterName, double value);
    Dictionary<string, double> GetCurrentPerformanceCounters();
}

/// <summary>
/// Represents a notification operation for monitoring
/// </summary>
public class NotificationOperation
{
    public NotificationOperationType OperationType { get; set; }
    public NotificationChannel Channel { get; set; }
    public OperationStatus Status { get; set; }
    public Guid? NotificationId { get; set; }
    public Guid? UserId { get; set; }
    public int AttemptNumber { get; set; } = 1;
    public TimeSpan? Duration { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Performance metrics for notifications
/// </summary>
public class PerformanceMetrics
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalOperations { get; set; }
    public int SuccessfulOperations { get; set; }
    public int FailedOperations { get; set; }
    public double SuccessRate { get; set; }
    public double AverageProcessingTime { get; set; }
    public Dictionary<string, int> OperationsByChannel { get; set; } = new();
    public Dictionary<string, int> OperationsByType { get; set; } = new();
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    public Dictionary<string, double> ProcessingTimePercentiles { get; set; } = new();
}

/// <summary>
/// Alert condition for monitoring
/// </summary>
public class AlertCondition
{
    public AlertType Type { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Performance counter for tracking metrics
/// </summary>
public class PerformanceCounter
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Types of notification operations
/// </summary>
public enum NotificationOperationType
{
    Creation,
    Dispatch,
    Retry,
    TemplateRendering,
    StatusUpdate
}

/// <summary>
/// Operation status
/// </summary>
public enum OperationStatus
{
    Success,
    Failed,
    Timeout,
    Cancelled
}

/// <summary>
/// Alert types
/// </summary>
public enum AlertType
{
    HighFailureRate,
    SlowProcessing,
    HighQueueDepth,
    CriticalFailure,
    ServiceUnavailable
}

/// <summary>
/// Alert severity levels
/// </summary>
public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}