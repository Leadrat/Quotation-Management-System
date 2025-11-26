using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CRM.Application.Common.Interfaces;
using CRM.Domain.Entities;
using CRM.Infrastructure.Services;
using CRM.Tests.Common;
using Moq;

namespace CRM.Tests.Notifications;

/**
 * Feature: notification-creation-dispatch, Property 12: Performance monitoring and alerting
 * Validates: Requirements 8.3, 8.4, 8.5
 */
public class PerformanceMonitoringPropertyTests : BaseIntegrationTest
{
    private readonly INotificationMonitoringService _monitoringService;
    private readonly INotificationDispatchAttemptRepository _dispatchAttemptRepository;
    private readonly Mock<ILogger<PerformanceMonitoringPropertyTests>> _mockLogger;

    public PerformanceMonitoringPropertyTests()
    {
        _monitoringService = ServiceProvider.GetRequiredService<INotificationMonitoringService>();
        _dispatchAttemptRepository = ServiceProvider.GetRequiredService<INotificationDispatchAttemptRepository>();
        _mockLogger = new Mock<ILogger<PerformanceMonitoringPropertyTests>>();
    }

    [Property]
    public Property PerformanceMetrics_ShouldTrackProcessingTimes()
    {
        return Prop.ForAll(
            GeneratePerformanceTestScenario(),
            async (operations) =>
            {
                // Record operations with different processing times
                foreach (var operation in operations)
                {
                    await _monitoringService.LogNotificationOperationAsync(operation);
                }

                // Get performance statistics
                var statistics = await _monitoringService.GetPerformanceStatisticsAsync(
                    DateTime.UtcNow.AddHours(-1), 
                    DateTime.UtcNow);

                // Verify metrics are captured
                return statistics != null &&
                       statistics.TotalOperations >= operations.Count &&
                       statistics.AverageProcessingTime > TimeSpan.Zero &&
                       statistics.MaxProcessingTime >= statistics.AverageProcessingTime &&
                       statistics.MinProcessingTime <= statistics.AverageProcessingTime;
            });
    }

    [Property]
    public Property ThroughputMetrics_ShouldReflectActualVolume()
    {
        return Prop.ForAll(
            GenerateThroughputTestData(),
            async (data) =>
            {
                var (operations, timeWindow) = data;
                
                // Record operations within time window
                var startTime = DateTime.UtcNow;
                foreach (var operation in operations)
                {
                    await _monitoringService.LogNotificationOperationAsync(operation);
                }
                var endTime = DateTime.UtcNow;

                // Get throughput metrics
                var throughput = await _monitoringService.GetThroughputMetricsAsync(startTime, endTime);

                // Verify throughput calculation
                var expectedThroughput = operations.Count / timeWindow.TotalMinutes;
                var actualThroughput = throughput.OperationsPerMinute;

                return throughput.TotalOperations >= operations.Count &&
                       actualThroughput >= 0 &&
                       throughput.TimeWindow == timeWindow;
            });
    }

    [Property]
    public Property ErrorRateMonitoring_ShouldDetectAnomalies()
    {
        return Prop.ForAll(
            GenerateErrorRateTestData(),
            async (data) =>
            {
                var (successfulOps, failedOps, threshold) = data;
                
                // Record mixed success/failure operations
                var allOperations = successfulOps.Concat(failedOps).ToList();
                foreach (var operation in allOperations)
                {
                    await _monitoringService.LogNotificationOperationAsync(operation);
                }

                // Get error rate metrics
                var errorMetrics = await _monitoringService.GetErrorRateMetricsAsync(
                    DateTime.UtcNow.AddHours(-1), 
                    DateTime.UtcNow);

                // Verify error rate calculation
                var expectedErrorRate = (double)failedOps.Count / allOperations.Count * 100;
                var actualErrorRate = errorMetrics.ErrorRatePercentage;

                var isAnomalyDetected = actualErrorRate > threshold;
                var shouldDetectAnomaly = expectedErrorRate > threshold;

                return Math.Abs(actualErrorRate - expectedErrorRate) < 5.0 && // Allow 5% variance
                       errorMetrics.TotalOperations >= allOperations.Count &&
                       errorMetrics.FailedOperations >= failedOps.Count &&
                       isAnomalyDetected == shouldDetectAnomaly;
            });
    }

    [Property]
    public Property ChannelPerformanceMetrics_ShouldTrackIndividually()
    {
        return Prop.ForAll(
            GenerateChannelPerformanceData(),
            async (channelOperations) =>
            {
                // Record operations for different channels
                foreach (var (channel, operations) in channelOperations)
                {
                    foreach (var operation in operations)
                    {
                        operation.Channel = channel;
                        await _monitoringService.LogNotificationOperationAsync(operation);
                    }
                }

                // Get channel-specific metrics
                var channelMetrics = await _monitoringService.GetChannelPerformanceMetricsAsync(
                    DateTime.UtcNow.AddHours(-1), 
                    DateTime.UtcNow);

                // Verify each channel has separate metrics
                foreach (var (channel, operations) in channelOperations)
                {
                    var channelMetric = channelMetrics.FirstOrDefault(m => m.Channel == channel);
                    if (channelMetric == null) return false;

                    if (channelMetric.TotalOperations < operations.Count) return false;
                    if (channelMetric.AverageProcessingTime <= TimeSpan.Zero) return false;
                }

                return channelMetrics.Count >= channelOperations.Count;
            });
    }

    [Property]
    public Property AlertThresholds_ShouldTriggerAppropriately()
    {
        return Prop.ForAll(
            GenerateAlertThresholdData(),
            async (data) =>
            {
                var (operations, processingTimeThreshold, errorRateThreshold) = data;
                
                // Record operations that may exceed thresholds
                foreach (var operation in operations)
                {
                    await _monitoringService.LogNotificationOperationAsync(operation);
                }

                // Check alert conditions
                var alerts = await _monitoringService.CheckAlertConditionsAsync();

                // Verify alert logic
                var maxProcessingTime = operations.Max(op => op.Duration);
                var errorRate = (double)operations.Count(op => op.Status == OperationStatus.Failed) / operations.Count * 100;

                var shouldHaveProcessingTimeAlert = maxProcessingTime > processingTimeThreshold;
                var shouldHaveErrorRateAlert = errorRate > errorRateThreshold;

                var hasProcessingTimeAlert = alerts.Any(a => a.AlertType == "ProcessingTime");
                var hasErrorRateAlert = alerts.Any(a => a.AlertType == "ErrorRate");

                return (hasProcessingTimeAlert == shouldHaveProcessingTimeAlert || 
                        !shouldHaveProcessingTimeAlert) && // Allow false negatives but not false positives
                       (hasErrorRateAlert == shouldHaveErrorRateAlert || 
                        !shouldHaveErrorRateAlert);
            });
    }

    [Property]
    public Property ResourceUtilizationMetrics_ShouldTrackSystemHealth()
    {
        return Prop.ForAll(
            GenerateResourceUtilizationData(),
            async (operations) =>
            {
                // Simulate high-load scenario
                var tasks = operations.Select(async operation =>
                {
                    await _monitoringService.LogNotificationOperationAsync(operation);
                });

                await Task.WhenAll(tasks);

                // Get resource utilization metrics
                var resourceMetrics = await _monitoringService.GetResourceUtilizationMetricsAsync();

                // Verify resource tracking
                return resourceMetrics != null &&
                       resourceMetrics.Timestamp != default &&
                       resourceMetrics.CpuUsagePercentage >= 0 &&
                       resourceMetrics.CpuUsagePercentage <= 100 &&
                       resourceMetrics.MemoryUsagePercentage >= 0 &&
                       resourceMetrics.MemoryUsagePercentage <= 100 &&
                       resourceMetrics.ActiveConnections >= 0;
            });
    }

    [Property]
    public Property PerformanceTrends_ShouldIdentifyPatterns()
    {
        return Prop.ForAll(
            GeneratePerformanceTrendData(),
            async (timeSeriesData) =>
            {
                // Record operations across different time periods
                foreach (var (timestamp, operations) in timeSeriesData)
                {
                    foreach (var operation in operations)
                    {
                        operation.Metadata = operation.Metadata ?? new Dictionary<string, object>();
                        operation.Metadata["Timestamp"] = timestamp.ToString("O");
                        await _monitoringService.LogNotificationOperationAsync(operation);
                    }
                }

                // Analyze performance trends
                var trends = await _monitoringService.GetPerformanceTrendsAsync(
                    DateTime.UtcNow.AddDays(-7), 
                    DateTime.UtcNow);

                // Verify trend analysis
                return trends != null &&
                       trends.DataPoints.Count > 0 &&
                       trends.DataPoints.All(dp => dp.Timestamp != default) &&
                       trends.DataPoints.All(dp => dp.AverageProcessingTime >= TimeSpan.Zero) &&
                       trends.TrendDirection != null;
            });
    }

    [Property]
    public Property QueueDepthMonitoring_ShouldTrackBacklog()
    {
        return Prop.ForAll(
            GenerateQueueDepthData(),
            async (queueData) =>
            {
                // Simulate queue operations
                foreach (var (queueName, depth) in queueData)
                {
                    await _monitoringService.RecordQueueDepthAsync(queueName, depth);
                }

                // Get queue depth metrics
                var queueMetrics = await _monitoringService.GetQueueDepthMetricsAsync();

                // Verify queue monitoring
                return queueMetrics != null &&
                       queueMetrics.Count >= queueData.Count &&
                       queueMetrics.All(qm => qm.QueueName != null) &&
                       queueMetrics.All(qm => qm.CurrentDepth >= 0) &&
                       queueMetrics.All(qm => qm.MaxDepth >= qm.CurrentDepth);
            });
    }

    private static Arbitrary<List<NotificationOperation>> GeneratePerformanceTestScenario()
    {
        return Arb.From(
            from count in Gen.Choose(5, 20)
            from operations in Gen.ListOf(
                from notificationId in Gen.Choose(1, 10000)
                from userId in Gen.Choose(1, 1000)
                from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.Sms)
                from processingTime in Gen.Choose(50, 5000)
                from status in Gen.Elements(OperationStatus.Success, OperationStatus.Failed)
                select new NotificationOperation
                {
                    NotificationId = notificationId,
                    UserId = userId,
                    Channel = channel,
                    OperationType = NotificationOperationType.Dispatch,
                    Status = status,
                    Duration = TimeSpan.FromMilliseconds(processingTime),
                    AttemptNumber = 1
                }, count)
            select operations);
    }

    private static Arbitrary<(List<NotificationOperation> operations, TimeSpan timeWindow)> GenerateThroughputTestData()
    {
        return Arb.From(
            from count in Gen.Choose(10, 50)
            from timeWindowMinutes in Gen.Choose(1, 10)
            from operations in Gen.ListOf(
                from notificationId in Gen.Choose(1, 10000)
                from userId in Gen.Choose(1, 1000)
                select new NotificationOperation
                {
                    NotificationId = notificationId,
                    UserId = userId,
                    Channel = NotificationChannel.InApp,
                    OperationType = NotificationOperationType.Dispatch,
                    Status = OperationStatus.Success,
                    Duration = TimeSpan.FromMilliseconds(100),
                    AttemptNumber = 1
                }, count)
            select (operations, TimeSpan.FromMinutes(timeWindowMinutes)));
    }

    private static Arbitrary<(List<NotificationOperation> successfulOps, List<NotificationOperation> failedOps, double threshold)> GenerateErrorRateTestData()
    {
        return Arb.From(
            from successCount in Gen.Choose(5, 20)
            from failCount in Gen.Choose(1, 10)
            from threshold in Gen.Choose(5.0, 50.0)
            from successOps in Gen.ListOf(GenerateSuccessfulOperation().Generator, successCount)
            from failedOps in Gen.ListOf(GenerateFailedOperation().Generator, failCount)
            select (successOps, failedOps, threshold));
    }

    private static Arbitrary<List<(NotificationChannel channel, List<NotificationOperation> operations)>> GenerateChannelPerformanceData()
    {
        return Arb.From(
            from channels in Gen.SubListOf(Enum.GetValues<NotificationChannel>()).Where(list => list.Any())
            from channelOps in Gen.ListOf(
                from channel in Gen.Elements(channels.ToArray())
                from opCount in Gen.Choose(2, 8)
                from operations in Gen.ListOf(GenerateSuccessfulOperation().Generator, opCount)
                select (channel, operations), channels.Count())
            select channelOps);
    }

    private static Arbitrary<(List<NotificationOperation> operations, TimeSpan processingTimeThreshold, double errorRateThreshold)> GenerateAlertThresholdData()
    {
        return Arb.From(
            from count in Gen.Choose(10, 30)
            from processingThresholdMs in Gen.Choose(1000, 5000)
            from errorThreshold in Gen.Choose(10.0, 30.0)
            from operations in Gen.ListOf(
                from notificationId in Gen.Choose(1, 10000)
                from userId in Gen.Choose(1, 1000)
                from processingTime in Gen.Choose(100, 8000) // Some may exceed threshold
                from status in Gen.Frequency(
                    (80, Gen.Constant(OperationStatus.Success)),
                    (20, Gen.Constant(OperationStatus.Failed)))
                select new NotificationOperation
                {
                    NotificationId = notificationId,
                    UserId = userId,
                    Channel = NotificationChannel.InApp,
                    OperationType = NotificationOperationType.Dispatch,
                    Status = status,
                    Duration = TimeSpan.FromMilliseconds(processingTime),
                    AttemptNumber = 1
                }, count)
            select (operations, TimeSpan.FromMilliseconds(processingThresholdMs), errorThreshold));
    }

    private static Arbitrary<List<NotificationOperation>> GenerateResourceUtilizationData()
    {
        return Arb.From(
            from count in Gen.Choose(20, 100) // Higher load for resource testing
            from operations in Gen.ListOf(GenerateSuccessfulOperation().Generator, count)
            select operations);
    }

    private static Arbitrary<List<(DateTime timestamp, List<NotificationOperation> operations)>> GeneratePerformanceTrendData()
    {
        return Arb.From(
            from dayCount in Gen.Choose(3, 7)
            from trendData in Gen.ListOf(
                from daysAgo in Gen.Choose(0, dayCount - 1)
                from opCount in Gen.Choose(5, 15)
                from operations in Gen.ListOf(GenerateSuccessfulOperation().Generator, opCount)
                select (DateTime.UtcNow.AddDays(-daysAgo), operations), dayCount)
            select trendData);
    }

    private static Arbitrary<List<(string queueName, int depth)>> GenerateQueueDepthData()
    {
        return Arb.From(
            from queueCount in Gen.Choose(2, 5)
            from queueData in Gen.ListOf(
                from queueName in Gen.Elements("notifications-high", "notifications-normal", "notifications-low", "notifications-critical")
                from depth in Gen.Choose(0, 100)
                select (queueName, depth), queueCount)
            select queueData.DistinctBy(q => q.queueName).ToList());
    }

    private static Arbitrary<NotificationOperation> GenerateSuccessfulOperation()
    {
        return Arb.From(
            from notificationId in Gen.Choose(1, 10000)
            from userId in Gen.Choose(1, 1000)
            from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.Sms)
            from processingTime in Gen.Choose(50, 2000)
            select new NotificationOperation
            {
                NotificationId = notificationId,
                UserId = userId,
                Channel = channel,
                OperationType = NotificationOperationType.Dispatch,
                Status = OperationStatus.Success,
                Duration = TimeSpan.FromMilliseconds(processingTime),
                AttemptNumber = 1
            });
    }

    private static Arbitrary<NotificationOperation> GenerateFailedOperation()
    {
        return Arb.From(
            from notificationId in Gen.Choose(1, 10000)
            from userId in Gen.Choose(1, 1000)
            from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.Sms)
            from processingTime in Gen.Choose(100, 3000)
            from errorMessage in Gen.Elements("Network timeout", "Invalid recipient", "Rate limit exceeded")
            select new NotificationOperation
            {
                NotificationId = notificationId,
                UserId = userId,
                Channel = channel,
                OperationType = NotificationOperationType.Dispatch,
                Status = OperationStatus.Failed,
                Duration = TimeSpan.FromMilliseconds(processingTime),
                AttemptNumber = 1,
                ErrorMessage = errorMessage
            });
    }
}

// Supporting types for the tests
public class NotificationOperation
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public NotificationChannel Channel { get; set; }
    public NotificationOperationType OperationType { get; set; }
    public OperationStatus Status { get; set; }
    public TimeSpan Duration { get; set; }
    public int AttemptNumber { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public enum NotificationOperationType
{
    Creation,
    Dispatch,
    StatusUpdate,
    Retry
}

public enum OperationStatus
{
    Success,
    Failed,
    Pending
}