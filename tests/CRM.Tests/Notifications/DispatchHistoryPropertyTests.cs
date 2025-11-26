using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.DependencyInjection;
using CRM.Application.Common.Interfaces;
using CRM.Domain.Entities;
using CRM.Tests.Common;

namespace CRM.Tests.Notifications;

/**
 * Feature: notification-creation-dispatch, Property 7: Complete dispatch history
 * Validates: Requirements 4.4, 4.5
 */
public class DispatchHistoryPropertyTests : BaseIntegrationTest
{
    private readonly INotificationDispatchAttemptRepository _dispatchAttemptRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationDispatchService _dispatchService;

    public DispatchHistoryPropertyTests()
    {
        _dispatchAttemptRepository = ServiceProvider.GetRequiredService<INotificationDispatchAttemptRepository>();
        _notificationRepository = ServiceProvider.GetRequiredService<INotificationRepository>();
        _dispatchService = ServiceProvider.GetRequiredService<INotificationDispatchService>();
    }

    [Property]
    public Property AllDispatchAttempts_ShouldBeRecordedInHistory()
    {
        return Prop.ForAll(
            GenerateNotificationWithDispatchAttempts(),
            async notification =>
            {
                // Create notification and dispatch
                await _notificationRepository.CreateAsync(notification);
                await _dispatchService.DispatchNotificationAsync(notification.Id);

                // Retrieve complete dispatch history
                var history = await _dispatchAttemptRepository.GetDispatchHistoryAsync(
                    notificationId: notification.Id);

                var historyCount = await _dispatchAttemptRepository.GetDispatchHistoryCountAsync(
                    notificationId: notification.Id);

                // Verify all attempts are recorded
                var expectedAttempts = notification.DispatchAttempts.Count;
                
                return history.Count() >= expectedAttempts &&
                       historyCount >= expectedAttempts &&
                       history.All(h => h.NotificationId == notification.Id) &&
                       history.All(h => h.AttemptedAt != default) &&
                       history.All(h => h.AttemptNumber > 0);
            });
    }

    [Property]
    public Property DispatchHistoryFiltering_ShouldRespectAllCriteria()
    {
        return Prop.ForAll(
            GenerateFilteredHistoryScenario(),
            async (data) =>
            {
                var (notifications, userId, channel, status, fromDate, toDate) = data;

                // Create notifications and dispatch them
                foreach (var notification in notifications)
                {
                    await _notificationRepository.CreateAsync(notification);
                    await _dispatchService.DispatchNotificationAsync(notification.Id);
                }

                // Apply filters and get history
                var filteredHistory = await _dispatchAttemptRepository.GetDispatchHistoryAsync(
                    userId: userId,
                    channel: channel,
                    status: status,
                    fromDate: fromDate,
                    toDate: toDate);

                var filteredCount = await _dispatchAttemptRepository.GetDispatchHistoryCountAsync(
                    userId: userId,
                    channel: channel,
                    status: status,
                    fromDate: fromDate,
                    toDate: toDate);

                // Verify filtering works correctly
                var matchesFilters = filteredHistory.All(h =>
                    (userId == null || h.UserId == userId) &&
                    (channel == null || h.Channel == channel) &&
                    (status == null || h.Status == status) &&
                    (fromDate == null || h.AttemptedAt >= fromDate) &&
                    (toDate == null || h.AttemptedAt <= toDate));

                return matchesFilters &&
                       filteredHistory.Count() <= filteredCount &&
                       filteredCount >= 0;
            });
    }

    [Property]
    public Property DispatchHistoryPagination_ShouldWorkCorrectly()
    {
        return Prop.ForAll(
            GeneratePaginationTestData(),
            async (data) =>
            {
                var (notifications, pageSize, pageNumber) = data;

                // Create notifications and dispatch them
                foreach (var notification in notifications)
                {
                    await _notificationRepository.CreateAsync(notification);
                    await _dispatchService.DispatchNotificationAsync(notification.Id);
                }

                // Get paginated history
                var paginatedHistory = await _dispatchAttemptRepository.GetDispatchHistoryAsync(
                    page: pageNumber,
                    pageSize: pageSize);

                var totalCount = await _dispatchAttemptRepository.GetDispatchHistoryCountAsync();

                // Verify pagination logic
                var expectedItemsOnPage = Math.Min(pageSize, Math.Max(0, totalCount - (pageNumber - 1) * pageSize));
                
                return paginatedHistory.Count() <= pageSize &&
                       paginatedHistory.Count() <= expectedItemsOnPage &&
                       (pageNumber == 1 || paginatedHistory.Count() <= totalCount) &&
                       paginatedHistory.All(h => h.Id > 0);
            });
    }

    [Property]
    public Property DispatchHistoryOrdering_ShouldBeConsistent()
    {
        return Prop.ForAll(
            GenerateOrderingTestData(),
            async notifications =>
            {
                // Create notifications with different timestamps
                foreach (var notification in notifications)
                {
                    await _notificationRepository.CreateAsync(notification);
                    await _dispatchService.DispatchNotificationAsync(notification.Id);
                }

                // Get history (should be ordered by most recent first)
                var history = await _dispatchAttemptRepository.GetDispatchHistoryAsync();

                // Verify ordering is consistent
                var historyList = history.ToList();
                var isOrderedByDateDesc = true;
                
                for (int i = 1; i < historyList.Count; i++)
                {
                    if (historyList[i - 1].AttemptedAt < historyList[i].AttemptedAt)
                    {
                        isOrderedByDateDesc = false;
                        break;
                    }
                }

                return isOrderedByDateDesc &&
                       historyList.All(h => h.AttemptedAt != default);
            });
    }

    [Property]
    public Property RetryHistory_ShouldMaintainSequentialRecord()
    {
        return Prop.ForAll(
            GenerateRetryHistoryScenario(),
            async notification =>
            {
                // Create notification and perform multiple dispatch attempts
                await _notificationRepository.CreateAsync(notification);
                
                // Initial dispatch
                await _dispatchService.DispatchNotificationAsync(notification.Id);
                
                // Simulate retries for failed attempts
                var failedAttempts = await _dispatchAttemptRepository.GetFailedAttemptsAsync();
                var notificationFailedAttempts = failedAttempts
                    .Where(fa => fa.NotificationId == notification.Id)
                    .Take(3); // Limit retries for test

                foreach (var failedAttempt in notificationFailedAttempts)
                {
                    await _dispatchService.RetryFailedDispatchAsync(failedAttempt.Id);
                }

                // Get complete history for this notification
                var history = await _dispatchAttemptRepository.GetDispatchHistoryAsync(
                    notificationId: notification.Id);

                // Verify retry sequence is properly recorded
                var attemptsByChannel = history.GroupBy(h => h.Channel);
                
                foreach (var channelGroup in attemptsByChannel)
                {
                    var attempts = channelGroup.OrderBy(a => a.AttemptNumber).ToList();
                    
                    // Verify sequential attempt numbers
                    for (int i = 0; i < attempts.Count; i++)
                    {
                        if (attempts[i].AttemptNumber != i + 1)
                            return false;
                    }
                    
                    // Verify timestamps are sequential
                    for (int i = 1; i < attempts.Count; i++)
                    {
                        if (attempts[i].AttemptedAt < attempts[i - 1].AttemptedAt)
                            return false;
                    }
                }

                return history.Any() && // At least some history exists
                       history.All(h => h.AttemptNumber > 0);
            });
    }

    [Property]
    public Property DispatchStatistics_ShouldMatchHistoryData()
    {
        return Prop.ForAll(
            GenerateStatisticsTestData(),
            async (notifications, fromDate, toDate) =>
            {
                // Create notifications and dispatch them
                foreach (var notification in notifications)
                {
                    await _notificationRepository.CreateAsync(notification);
                    await _dispatchService.DispatchNotificationAsync(notification.Id);
                }

                // Get statistics and history for the same period
                var statistics = await _dispatchAttemptRepository.GetStatisticsAsync(fromDate, toDate);
                var history = await _dispatchAttemptRepository.GetDispatchHistoryAsync(
                    fromDate: fromDate,
                    toDate: toDate);

                // Verify statistics match history data
                var historyList = history.ToList();
                var expectedTotal = historyList.Count;
                var expectedSuccessful = historyList.Count(h => h.Status == DispatchStatus.Delivered);
                var expectedFailed = historyList.Count(h => h.Status == DispatchStatus.Failed);
                var expectedPending = historyList.Count(h => h.Status == DispatchStatus.Pending);

                return statistics.TotalAttempts >= expectedTotal &&
                       statistics.SuccessfulDeliveries >= expectedSuccessful &&
                       statistics.FailedAttempts >= expectedFailed &&
                       statistics.PendingAttempts >= expectedPending &&
                       statistics.SuccessRate >= 0 && statistics.SuccessRate <= 100;
            });
    }

    [Property]
    public Property ChannelSpecificHistory_ShouldIsolateCorrectly()
    {
        return Prop.ForAll(
            GenerateChannelHistoryData(),
            async channelNotifications =>
            {
                // Create notifications for different channels
                foreach (var (channel, notifications) in channelNotifications)
                {
                    foreach (var notification in notifications)
                    {
                        // Ensure dispatch attempts are for the correct channel
                        foreach (var attempt in notification.DispatchAttempts)
                        {
                            attempt.Channel = channel;
                        }
                        
                        await _notificationRepository.CreateAsync(notification);
                        await _dispatchService.DispatchNotificationAsync(notification.Id);
                    }
                }

                // Verify channel isolation in history
                foreach (var (channel, notifications) in channelNotifications)
                {
                    var channelHistory = await _dispatchAttemptRepository.GetDispatchHistoryAsync(
                        channel: channel);

                    var channelCount = await _dispatchAttemptRepository.GetDispatchHistoryCountAsync(
                        channel: channel);

                    // All history entries should be for the specified channel
                    var allMatchChannel = channelHistory.All(h => h.Channel == channel);
                    var expectedMinCount = notifications.Sum(n => n.DispatchAttempts.Count(da => da.Channel == channel));

                    if (!allMatchChannel || channelHistory.Count() < expectedMinCount)
                        return false;
                }

                return true;
            });
    }

    [Property]
    public Property DispatchHistorySearch_ShouldFindRelevantEntries()
    {
        return Prop.ForAll(
            GenerateSearchTestData(),
            async (notifications, searchUserId) =>
            {
                // Create notifications for different users
                foreach (var notification in notifications)
                {
                    await _notificationRepository.CreateAsync(notification);
                    await _dispatchService.DispatchNotificationAsync(notification.Id);
                }

                // Search for specific user's dispatch history
                var userHistory = await _dispatchAttemptRepository.GetDispatchHistoryAsync(
                    userId: searchUserId);

                var userCount = await _dispatchAttemptRepository.GetDispatchHistoryCountAsync(
                    userId: searchUserId);

                // Verify search results
                var allBelongToUser = userHistory.All(h => h.UserId == searchUserId);
                var expectedMinCount = notifications.Count(n => n.UserId == searchUserId);

                return allBelongToUser &&
                       userHistory.Count() <= userCount &&
                       (expectedMinCount == 0 || userHistory.Any()); // If we expect results, we should get some
            });
    }

    private static Arbitrary<Notification> GenerateNotificationWithDispatchAttempts()
    {
        return Arb.From(
            from userId in Gen.Choose(1, 100)
            from channels in Gen.SubListOf(Enum.GetValues<NotificationChannel>()).Where(list => list.Any())
            select new Notification
            {
                UserId = userId,
                Title = "Test Notification",
                Message = "Test message for dispatch history",
                NotificationType = "test",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                DispatchAttempts = channels.Select(channel => new NotificationDispatchAttempt
                {
                    UserId = userId,
                    Channel = channel,
                    Status = DispatchStatus.Pending,
                    AttemptNumber = 1,
                    AttemptedAt = DateTime.UtcNow.AddMinutes(-Random.Shared.Next(1, 60)),
                    RenderedSubject = channel == NotificationChannel.Sms ? null : "Test Subject",
                    RenderedBody = "Test Body"
                }).ToList()
            });
    }

    private static Arbitrary<(List<Notification> notifications, int? userId, NotificationChannel? channel, DispatchStatus? status, DateTime? fromDate, DateTime? toDate)> GenerateFilteredHistoryScenario()
    {
        return Arb.From(
            from count in Gen.Choose(3, 10)
            from notifications in Gen.ListOf(GenerateNotificationWithDispatchAttempts().Generator, count)
            from hasUserFilter in Arb.Generate<bool>()
            from hasChannelFilter in Arb.Generate<bool>()
            from hasStatusFilter in Arb.Generate<bool>()
            from hasDateFilter in Arb.Generate<bool>()
            from userId in hasUserFilter ? Gen.Choose(1, 100).Select(id => (int?)id) : Gen.Constant((int?)null)
            from channel in hasChannelFilter ? Gen.Elements(Enum.GetValues<NotificationChannel>()).Select(c => (NotificationChannel?)c) : Gen.Constant((NotificationChannel?)null)
            from status in hasStatusFilter ? Gen.Elements(Enum.GetValues<DispatchStatus>()).Select(s => (DispatchStatus?)s) : Gen.Constant((DispatchStatus?)null)
            from fromDate in hasDateFilter ? Gen.Elements(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(-3)).Select(d => (DateTime?)d) : Gen.Constant((DateTime?)null)
            from toDate in hasDateFilter ? Gen.Elements(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow).Select(d => (DateTime?)d) : Gen.Constant((DateTime?)null)
            select (notifications, userId, channel, status, fromDate, toDate));
    }

    private static Arbitrary<(List<Notification> notifications, int pageSize, int pageNumber)> GeneratePaginationTestData()
    {
        return Arb.From(
            from count in Gen.Choose(5, 25)
            from notifications in Gen.ListOf(GenerateNotificationWithDispatchAttempts().Generator, count)
            from pageSize in Gen.Choose(5, 15)
            from pageNumber in Gen.Choose(1, 5)
            select (notifications, pageSize, pageNumber));
    }

    private static Arbitrary<List<Notification>> GenerateOrderingTestData()
    {
        return Arb.From(
            from count in Gen.Choose(5, 15)
            from notifications in Gen.ListOf(
                from userId in Gen.Choose(1, 100)
                from minutesAgo in Gen.Choose(1, 120)
                select new Notification
                {
                    UserId = userId,
                    Title = "Ordering Test",
                    Message = "Test message for ordering",
                    NotificationType = "test",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-minutesAgo),
                    IsRead = false,
                    DispatchAttempts = new List<NotificationDispatchAttempt>
                    {
                        new NotificationDispatchAttempt
                        {
                            UserId = userId,
                            Channel = NotificationChannel.InApp,
                            Status = DispatchStatus.Pending,
                            AttemptNumber = 1,
                            AttemptedAt = DateTime.UtcNow.AddMinutes(-minutesAgo),
                            RenderedSubject = "Test Subject",
                            RenderedBody = "Test Body"
                        }
                    }
                }, count)
            select notifications);
    }

    private static Arbitrary<Notification> GenerateRetryHistoryScenario()
    {
        return Arb.From(
            from userId in Gen.Choose(1, 100)
            select new Notification
            {
                UserId = userId,
                Title = "Retry Test",
                Message = "Test message for retry history",
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

    private static Arbitrary<(List<Notification> notifications, DateTime? fromDate, DateTime? toDate)> GenerateStatisticsTestData()
    {
        return Arb.From(
            from count in Gen.Choose(5, 15)
            from notifications in Gen.ListOf(GenerateNotificationWithDispatchAttempts().Generator, count)
            from hasDateRange in Arb.Generate<bool>()
            from fromDate in hasDateRange ? Gen.Elements(DateTime.UtcNow.AddDays(-7)).Select(d => (DateTime?)d) : Gen.Constant((DateTime?)null)
            from toDate in hasDateRange ? Gen.Elements(DateTime.UtcNow).Select(d => (DateTime?)d) : Gen.Constant((DateTime?)null)
            select (notifications, fromDate, toDate));
    }

    private static Arbitrary<List<(NotificationChannel channel, List<Notification> notifications)>> GenerateChannelHistoryData()
    {
        return Arb.From(
            from channels in Gen.SubListOf(Enum.GetValues<NotificationChannel>()).Where(list => list.Any())
            from channelData in Gen.ListOf(
                from channel in Gen.Elements(channels.ToArray())
                from count in Gen.Choose(2, 5)
                from notifications in Gen.ListOf(GenerateNotificationWithDispatchAttempts().Generator, count)
                select (channel, notifications), channels.Count())
            select channelData);
    }

    private static Arbitrary<(List<Notification> notifications, int searchUserId)> GenerateSearchTestData()
    {
        return Arb.From(
            from searchUserId in Gen.Choose(1, 50)
            from count in Gen.Choose(5, 15)
            from notifications in Gen.ListOf(
                from userId in Gen.Frequency(
                    (30, Gen.Constant(searchUserId)), // 30% chance of matching user
                    (70, Gen.Choose(1, 100))) // 70% chance of other users
                from notification in GenerateNotificationWithDispatchAttempts().Generator
                select new Notification
                {
                    UserId = userId,
                    Title = notification.Title,
                    Message = notification.Message,
                    NotificationType = notification.NotificationType,
                    CreatedAt = notification.CreatedAt,
                    IsRead = notification.IsRead,
                    DispatchAttempts = notification.DispatchAttempts.Select(da => new NotificationDispatchAttempt
                    {
                        UserId = userId, // Ensure dispatch attempt matches notification user
                        Channel = da.Channel,
                        Status = da.Status,
                        AttemptNumber = da.AttemptNumber,
                        AttemptedAt = da.AttemptedAt,
                        RenderedSubject = da.RenderedSubject,
                        RenderedBody = da.RenderedBody
                    }).ToList()
                }, count)
            select (notifications, searchUserId));
    }
}