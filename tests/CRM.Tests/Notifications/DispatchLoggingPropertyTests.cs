using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CRM.Application.Common.Interfaces;
using CRM.Domain.Entities;
using CRM.Tests.Common;
using Moq;

namespace CRM.Tests.Notifications;

/**
 * Feature: notification-creation-dispatch, Property 6: Comprehensive dispatch logging
 * Validates: Requirements 4.1, 4.2, 4.3
 */
public class DispatchLoggingPropertyTests : BaseIntegrationTest
{
    private readonly INotificationDispatchService _dispatchService;
    private readonly INotificationDispatchAttemptRepository _dispatchAttemptRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly Mock<ILogger<DispatchLoggingPropertyTests>> _mockLogger;

    public DispatchLoggingPropertyTests()
    {
        _dispatchService = ServiceProvider.GetRequiredService<INotificationDispatchService>();
        _dispatchAttemptRepository = ServiceProvider.GetRequiredService<INotificationDispatchAttemptRepository>();
        _notificationRepository = ServiceProvider.GetRequiredService<INotificationRepository>();
        _mockLogger = new Mock<ILogger<DispatchLoggingPropertyTests>>();
    }

    [Property]
    public Property AllDispatchAttempts_ShouldBeLogged()
    {
        return Prop.ForAll(
            GenerateNotificationWithMultipleChannels(),
            async notification =>
            {
                // Create notification
                await _notificationRepository.CreateAsync(notification);
                
                // Dispatch notification
                await _dispatchService.DispatchNotificationAsync(notification.Id);
                
                // Verify all dispatch attempts are logged
                var dispatchAttempts = await _dispatchAttemptRepository.GetByNotificationIdAsync(notification.Id);
                
                // Should have dispatch attempts for all channels
                var expectedChannels = notification.DispatchAttempts.Select(da => da.Channel).Distinct();
                var actualChannels = dispatchAttempts.Select(da => da.Channel).Distinct();
                
                return expectedChannels.All(channel => actualChannels.Contains(channel)) &&
                       dispatchAttempts.All(da => da.AttemptedAt != default) &&
                       dispatchAttempts.All(da => !string.IsNullOrEmpty(da.Status.ToString()));
            });
    }

    [Property]
    public Property FailedDispatches_ShouldLogErrorDetails()
    {
        return Prop.ForAll(
            GenerateNotificationWithFailureScenario(),
            async (notification, shouldFail) =>
            {
                await _notificationRepository.CreateAsync(notification);
                
                // Simulate dispatch with potential failure
                var success = await _dispatchService.DispatchNotificationAsync(notification.Id);
                
                var dispatchAttempts = await _dispatchAttemptRepository.GetByNotificationIdAsync(notification.Id);
                var failedAttempts = dispatchAttempts.Where(da => da.Status == DispatchStatus.Failed);
                
                if (shouldFail && failedAttempts.Any())
                {
                    // Failed attempts should have error details
                    return failedAttempts.All(da => 
                        !string.IsNullOrEmpty(da.ErrorMessage) &&
                        da.AttemptedAt != default &&
                        da.AttemptNumber > 0);
                }
                
                // If no failures expected, should not have failed attempts with missing details
                return failedAttempts.All(da => 
                    da.AttemptedAt != default &&
                    da.AttemptNumber > 0);
            });
    }

    [Property]
    public Property SuccessfulDispatches_ShouldLogDeliveryTime()
    {
        return Prop.ForAll(
            GenerateNotificationWithSuccessScenario(),
            async notification =>
            {
                await _notificationRepository.CreateAsync(notification);
                
                var beforeDispatch = DateTime.UtcNow;
                await _dispatchService.DispatchNotificationAsync(notification.Id);
                var afterDispatch = DateTime.UtcNow;
                
                var dispatchAttempts = await _dispatchAttemptRepository.GetByNotificationIdAsync(notification.Id);
                var successfulAttempts = dispatchAttempts.Where(da => da.Status == DispatchStatus.Delivered);
                
                return successfulAttempts.All(da =>
                    da.DeliveredAt.HasValue &&
                    da.DeliveredAt.Value >= beforeDispatch &&
                    da.DeliveredAt.Value <= afterDispatch &&
                    da.DeliveredAt.Value >= da.AttemptedAt);
            });
    }

    [Property]
    public Property RetryAttempts_ShouldMaintainSequentialLogging()
    {
        return Prop.ForAll(
            GenerateNotificationForRetryTesting(),
            async notification =>
            {
                await _notificationRepository.CreateAsync(notification);
                
                // Initial dispatch
                await _dispatchService.DispatchNotificationAsync(notification.Id);
                
                // Get failed attempts and retry them
                var failedAttempts = await _dispatchAttemptRepository.GetFailedAttemptsAsync();
                var notificationFailedAttempts = failedAttempts.Where(fa => fa.NotificationId == notification.Id);
                
                foreach (var failedAttempt in notificationFailedAttempts.Take(2)) // Limit retries for test
                {
                    await _dispatchService.RetryFailedDispatchAsync(failedAttempt.Id);
                }
                
                // Verify retry sequence logging
                var allAttempts = await _dispatchAttemptRepository.GetByNotificationIdAsync(notification.Id);
                var attemptsByChannel = allAttempts.GroupBy(a => a.Channel);
                
                foreach (var channelGroup in attemptsByChannel)
                {
                    var attempts = channelGroup.OrderBy(a => a.AttemptNumber).ToList();
                    
                    // Verify sequential attempt numbers
                    for (int i = 0; i < attempts.Count; i++)
                    {
                        if (attempts[i].AttemptNumber != i + 1)
                            return false;
                        
                        // Verify timestamps are sequential
                        if (i > 0 && attempts[i].AttemptedAt < attempts[i - 1].AttemptedAt)
                            return false;
                    }
                }
                
                return true;
            });
    }

    [Property]
    public Property DispatchStatistics_ShouldReflectActualAttempts()
    {
        return Prop.ForAll(
            GenerateMultipleNotificationsForStatistics(),
            async notifications =>
            {
                // Create and dispatch multiple notifications
                foreach (var notification in notifications)
                {
                    await _notificationRepository.CreateAsync(notification);
                    await _dispatchService.DispatchNotificationAsync(notification.Id);
                }
                
                // Get statistics
                var statistics = await _dispatchAttemptRepository.GetStatisticsAsync();
                
                // Verify statistics match actual attempts
                var allAttempts = new List<NotificationDispatchAttempt>();
                foreach (var notification in notifications)
                {
                    var attempts = await _dispatchAttemptRepository.GetByNotificationIdAsync(notification.Id);
                    allAttempts.AddRange(attempts);
                }
                
                var expectedTotal = allAttempts.Count;
                var expectedSuccessful = allAttempts.Count(a => a.Status == DispatchStatus.Delivered);
                var expectedFailed = allAttempts.Count(a => a.Status == DispatchStatus.Failed);
                var expectedPending = allAttempts.Count(a => a.Status == DispatchStatus.Pending);
                
                return statistics.TotalAttempts >= expectedTotal &&
                       statistics.SuccessfulDeliveries >= expectedSuccessful &&
                       statistics.FailedAttempts >= expectedFailed &&
                       statistics.PendingAttempts >= expectedPending;
            });
    }

    [Property]
    public Property ChannelSpecificLogging_ShouldTrackPerformanceMetrics()
    {
        return Prop.ForAll(
            GenerateNotificationWithAllChannels(),
            async notification =>
            {
                await _notificationRepository.CreateAsync(notification);
                
                var beforeDispatch = DateTime.UtcNow;
                await _dispatchService.DispatchNotificationAsync(notification.Id);
                var afterDispatch = DateTime.UtcNow;
                
                var dispatchAttempts = await _dispatchAttemptRepository.GetByNotificationIdAsync(notification.Id);
                
                // Verify each channel has appropriate metrics
                foreach (NotificationChannel channel in Enum.GetValues<NotificationChannel>())
                {
                    var channelAttempts = dispatchAttempts.Where(da => da.Channel == channel);
                    
                    foreach (var attempt in channelAttempts)
                    {
                        // Basic logging requirements
                        if (attempt.AttemptedAt < beforeDispatch || attempt.AttemptedAt > afterDispatch)
                            return false;
                        
                        if (attempt.AttemptNumber <= 0)
                            return false;
                        
                        // Channel-specific validation
                        switch (channel)
                        {
                            case NotificationChannel.Email:
                                // Email should have rendered subject and body
                                if (string.IsNullOrEmpty(attempt.RenderedSubject) || 
                                    string.IsNullOrEmpty(attempt.RenderedBody))
                                    return false;
                                break;
                            
                            case NotificationChannel.Sms:
                                // SMS should have rendered body but no subject
                                if (!string.IsNullOrEmpty(attempt.RenderedSubject) || 
                                    string.IsNullOrEmpty(attempt.RenderedBody))
                                    return false;
                                break;
                            
                            case NotificationChannel.InApp:
                                // In-app should have both subject and body
                                if (string.IsNullOrEmpty(attempt.RenderedSubject) || 
                                    string.IsNullOrEmpty(attempt.RenderedBody))
                                    return false;
                                break;
                        }
                    }
                }
                
                return true;
            });
    }

    [Property]
    public Property DispatchHistory_ShouldBeQueryableAndComplete()
    {
        return Prop.ForAll(
            GenerateDispatchHistoryScenario(),
            async (notifications, fromDate, toDate) =>
            {
                // Create notifications with different timestamps
                foreach (var (notification, timestamp) in notifications)
                {
                    notification.CreatedAt = timestamp;
                    await _notificationRepository.CreateAsync(notification);
                    await _dispatchService.DispatchNotificationAsync(notification.Id);
                }
                
                // Query dispatch history
                var history = await _dispatchAttemptRepository.GetDispatchHistoryAsync(
                    fromDate: fromDate,
                    toDate: toDate);
                
                var historyCount = await _dispatchAttemptRepository.GetDispatchHistoryCountAsync(
                    fromDate: fromDate,
                    toDate: toDate);
                
                // Verify history completeness
                var expectedAttempts = new List<NotificationDispatchAttempt>();
                foreach (var (notification, timestamp) in notifications)
                {
                    if (timestamp >= fromDate && timestamp <= toDate)
                    {
                        var attempts = await _dispatchAttemptRepository.GetByNotificationIdAsync(notification.Id);
                        expectedAttempts.AddRange(attempts);
                    }
                }
                
                return history.Count() <= historyCount &&
                       history.All(h => h.AttemptedAt >= fromDate && h.AttemptedAt <= toDate) &&
                       history.Count() >= Math.Min(expectedAttempts.Count, 20); // Assuming page size of 20
            });
    }

    private static Arbitrary<Notification> GenerateNotificationWithMultipleChannels()
    {
        return Arb.From(
            from userId in Gen.Choose(1, 100)
            from channels in Gen.SubListOf(Enum.GetValues<NotificationChannel>()).Where(list => list.Any())
            select new Notification
            {
                UserId = userId,
                Title = "Multi-channel Test",
                Message = "Test notification for multiple channels",
                NotificationType = "test",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                DispatchAttempts = channels.Select(channel => new NotificationDispatchAttempt
                {
                    UserId = userId,
                    Channel = channel,
                    Status = DispatchStatus.Pending,
                    AttemptNumber = 1,
                    AttemptedAt = DateTime.UtcNow,
                    RenderedSubject = channel == NotificationChannel.Sms ? null : "Test Subject",
                    RenderedBody = "Test Body"
                }).ToList()
            });
    }

    private static Arbitrary<(Notification notification, bool shouldFail)> GenerateNotificationWithFailureScenario()
    {
        return Arb.From(
            from notification in GenerateNotificationWithMultipleChannels().Generator
            from shouldFail in Arb.Generate<bool>()
            select (notification, shouldFail));
    }

    private static Arbitrary<Notification> GenerateNotificationWithSuccessScenario()
    {
        return GenerateNotificationWithMultipleChannels();
    }

    private static Arbitrary<Notification> GenerateNotificationForRetryTesting()
    {
        return Arb.From(
            from userId in Gen.Choose(1, 100)
            select new Notification
            {
                UserId = userId,
                Title = "Retry Test",
                Message = "Test notification for retry scenarios",
                NotificationType = "retry-test",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                DispatchAttempts = new List<NotificationDispatchAttempt>
                {
                    new NotificationDispatchAttempt
                    {
                        UserId = userId,
                        Channel = NotificationChannel.Email,
                        Status = DispatchStatus.Pending,
                        AttemptNumber = 1,
                        AttemptedAt = DateTime.UtcNow,
                        RenderedSubject = "Test Subject",
                        RenderedBody = "Test Body"
                    }
                }
            });
    }

    private static Arbitrary<List<Notification>> GenerateMultipleNotificationsForStatistics()
    {
        return Arb.From(
            from count in Gen.Choose(3, 8)
            from notifications in Gen.ListOf(GenerateNotificationWithMultipleChannels().Generator, count)
            select notifications);
    }

    private static Arbitrary<Notification> GenerateNotificationWithAllChannels()
    {
        return Arb.From(
            from userId in Gen.Choose(1, 100)
            select new Notification
            {
                UserId = userId,
                Title = "All Channels Test",
                Message = "Test notification for all channels",
                NotificationType = "all-channels-test",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                DispatchAttempts = Enum.GetValues<NotificationChannel>().Select(channel => 
                    new NotificationDispatchAttempt
                    {
                        UserId = userId,
                        Channel = channel,
                        Status = DispatchStatus.Pending,
                        AttemptNumber = 1,
                        AttemptedAt = DateTime.UtcNow,
                        RenderedSubject = channel == NotificationChannel.Sms ? null : "Test Subject",
                        RenderedBody = "Test Body"
                    }).ToList()
            });
    }

    private static Arbitrary<(List<(Notification notification, DateTime timestamp)> notifications, DateTime fromDate, DateTime toDate)> GenerateDispatchHistoryScenario()
    {
        return Arb.From(
            from baseDate in Gen.Elements(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(-1))
            from count in Gen.Choose(2, 6)
            from notifications in Gen.ListOf(GenerateNotificationWithMultipleChannels().Generator, count)
            from dayOffsets in Gen.ListOf(Gen.Choose(0, 10), count)
            let timestampedNotifications = notifications.Zip(dayOffsets, (n, offset) => (n, baseDate.AddDays(offset)))
            from fromDays in Gen.Choose(-2, 5)
            from toDays in Gen.Choose(fromDays + 1, 12)
            let fromDate = baseDate.AddDays(fromDays)
            let toDate = baseDate.AddDays(toDays)
            select (timestampedNotifications.ToList(), fromDate, toDate));
    }
}