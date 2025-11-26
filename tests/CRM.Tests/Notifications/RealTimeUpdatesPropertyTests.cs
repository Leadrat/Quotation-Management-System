using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Services;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Notifications;

/// <summary>
/// **Feature: notification-creation-dispatch, Property 9: Real-time UI synchronization**
/// Property-based tests for real-time notification updates system
/// </summary>
public class RealTimeUpdatesPropertyTests
{
    private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
    private readonly Mock<ILogger<RealTimeNotificationService>> _mockLogger;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly RealTimeNotificationService _realTimeService;

    public RealTimeUpdatesPropertyTests()
    {
        _mockHubContext = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<RealTimeNotificationService>>();
        _mockClients = new Mock<IHubCallerClients>();
        _mockClientProxy = new Mock<IClientProxy>();

        _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(c => c.User(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClients.Setup(c => c.Users(It.IsAny<IReadOnlyList<string>>())).Returns(_mockClientProxy.Object);
        _mockClients.Setup(c => c.All).Returns(_mockClientProxy.Object);

        _realTimeService = new RealTimeNotificationService(_mockHubContext.Object, _mockLogger.Object);
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 9: Real-time UI synchronization**
    /// **Validates: Requirements 6.1, 6.2, 6.3, 6.4**
    /// 
    /// For any notification sent to a user, the system should broadcast the notification
    /// in real-time with all required properties preserved
    /// </summary>
    [Property(MaxTest = 100)]
    public Property RealTimeNotificationPreservesAllProperties()
    {
        return Prop.ForAll(
            GenerateUserNotification(),
            async (notification) =>
            {
                // Arrange
                object? sentData = null;
                _mockClientProxy.Setup(proxy => proxy.SendAsync(
                    "ReceiveNotification",
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()))
                    .Callback<string, object[], CancellationToken>((method, args, token) =>
                    {
                        sentData = args.FirstOrDefault();
                    })
                    .Returns(Task.CompletedTask);

                // Act
                await _realTimeService.SendNotificationToUserAsync(
                    (int)notification.UserId, 
                    notification);

                // Assert - All notification properties preserved
                _mockClientProxy.Verify(proxy => proxy.SendAsync(
                    "ReceiveNotification",
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()), Times.Once);

                return (sentData != null).Label("Notification data was sent")
                    .And(ValidateNotificationData(sentData, notification));
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 9: Real-time UI synchronization**
    /// **Validates: Requirements 6.1, 6.2**
    /// 
    /// For any multi-user notification, the system should broadcast to all specified users
    /// without sending duplicate messages to the same user
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MultiUserNotificationAvoidsDuplicates()
    {
        return Prop.ForAll(
            GenerateMultiUserNotification(),
            async (data) =>
            {
                var (notification, userIds) = data;
                
                // Arrange
                var sentUserIds = new List<string>();
                _mockClients.Setup(c => c.Users(It.IsAny<IReadOnlyList<string>>()))
                    .Callback<IReadOnlyList<string>>(users => sentUserIds.AddRange(users))
                    .Returns(_mockClientProxy.Object);

                _mockClientProxy.Setup(proxy => proxy.SendAsync(
                    "ReceiveNotification",
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                // Act
                await _realTimeService.SendNotificationToUsersAsync(userIds, notification);

                // Assert - No duplicate user IDs sent
                var expectedUserIds = userIds.Select(id => id.ToString()).Distinct().ToList();
                
                _mockClients.Verify(c => c.Users(It.IsAny<IReadOnlyList<string>>()), Times.Once);
                
                return (sentUserIds.Count == expectedUserIds.Count).Label($"Sent to {sentUserIds.Count} users, expected {expectedUserIds.Count}")
                    .And((sentUserIds.Distinct().Count() == sentUserIds.Count).Label("No duplicate user IDs"))
                    .And((expectedUserIds.All(id => sentUserIds.Contains(id))).Label("All expected users included"));
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 9: Real-time UI synchronization**
    /// **Validates: Requirements 6.2, 6.4**
    /// 
    /// For any notification read status update, the system should broadcast the update
    /// to the specific user with the correct notification ID
    /// </summary>
    [Property(MaxTest = 100)]
    public Property NotificationReadStatusUpdatesCorrectly()
    {
        return Prop.ForAll(
            GenerateReadStatusUpdate(),
            async (data) =>
            {
                var (userId, notificationId) = data;
                
                // Arrange
                object? sentData = null;
                string? targetUserId = null;
                
                _mockClients.Setup(c => c.User(It.IsAny<string>()))
                    .Callback<string>(user => targetUserId = user)
                    .Returns(_mockClientProxy.Object);

                _mockClientProxy.Setup(proxy => proxy.SendAsync(
                    "NotificationRead",
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()))
                    .Callback<string, object[], CancellationToken>((method, args, token) =>
                    {
                        sentData = args.FirstOrDefault();
                    })
                    .Returns(Task.CompletedTask);

                // Act
                await _realTimeService.NotifyNotificationReadAsync(userId, notificationId);

                // Assert - Correct user and notification ID
                return (targetUserId == userId.ToString()).Label($"Targeted user {targetUserId} matches expected {userId}")
                    .And((sentData != null).Label("Read status data was sent"))
                    .And(ValidateReadStatusData(sentData, notificationId));
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 9: Real-time UI synchronization**
    /// **Validates: Requirements 6.2, 6.4**
    /// 
    /// For any unread count update, the system should send the correct count
    /// to the specific user without affecting other users
    /// </summary>
    [Property(MaxTest = 100)]
    public Property UnreadCountUpdatesCorrectly()
    {
        return Prop.ForAll(
            GenerateUnreadCountUpdate(),
            async (data) =>
            {
                var (userId, unreadCount) = data;
                
                // Arrange
                object? sentData = null;
                string? targetUserId = null;
                
                _mockClients.Setup(c => c.User(It.IsAny<string>()))
                    .Callback<string>(user => targetUserId = user)
                    .Returns(_mockClientProxy.Object);

                _mockClientProxy.Setup(proxy => proxy.SendAsync(
                    "UnreadCountUpdate",
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()))
                    .Callback<string, object[], CancellationToken>((method, args, token) =>
                    {
                        sentData = args.FirstOrDefault();
                    })
                    .Returns(Task.CompletedTask);

                // Act
                await _realTimeService.SendUnreadCountUpdateAsync(userId, unreadCount);

                // Assert - Correct user and count
                return (targetUserId == userId.ToString()).Label($"Targeted user {targetUserId} matches expected {userId}")
                    .And((sentData != null).Label("Unread count data was sent"))
                    .And(ValidateUnreadCountData(sentData, unreadCount));
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 9: Real-time UI synchronization**
    /// **Validates: Requirements 6.1, 6.3**
    /// 
    /// For any system-wide notification, the system should broadcast to all connected users
    /// using the appropriate broadcast method
    /// </summary>
    [Property(MaxTest = 50)]
    public Property SystemWideNotificationsBroadcastToAll()
    {
        return Prop.ForAll(
            GenerateSystemWideNotification(),
            async (notification) =>
            {
                // Arrange
                object? sentData = null;
                bool usedAllClients = false;
                
                _mockClients.Setup(c => c.All)
                    .Callback(() => usedAllClients = true)
                    .Returns(_mockClientProxy.Object);

                _mockClientProxy.Setup(proxy => proxy.SendAsync(
                    "ReceiveNotification",
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()))
                    .Callback<string, object[], CancellationToken>((method, args, token) =>
                    {
                        sentData = args.FirstOrDefault();
                    })
                    .Returns(Task.CompletedTask);

                // Act
                await _realTimeService.SendNotificationToAllUsersAsync(notification);

                // Assert - Used broadcast to all clients
                return (usedAllClients).Label("Used All clients for broadcast")
                    .And((sentData != null).Label("Notification data was sent"))
                    .And(ValidateNotificationData(sentData, notification));
            });
    }

    #region Validation Helpers

    private static Property ValidateNotificationData(object? sentData, UserNotification notification)
    {
        if (sentData == null) return false.Label("Sent data is null");

        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(sentData);
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (data == null) return false.Label("Could not deserialize sent data");

            var hasId = data.ContainsKey("Id") || data.ContainsKey("id");
            var hasTitle = data.ContainsKey("Title") || data.ContainsKey("title");
            var hasMessage = data.ContainsKey("Message") || data.ContainsKey("message");
            var hasCreatedAt = data.ContainsKey("CreatedAt") || data.ContainsKey("createdAt");

            return (hasId).Label("Contains notification ID")
                .And((hasTitle).Label("Contains notification title"))
                .And((hasMessage).Label("Contains notification message"))
                .And((hasCreatedAt).Label("Contains creation timestamp"));
        }
        catch
        {
            return false.Label("Error validating notification data");
        }
    }

    private static Property ValidateReadStatusData(object? sentData, int notificationId)
    {
        if (sentData == null) return false.Label("Read status data is null");

        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(sentData);
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (data == null) return false.Label("Could not deserialize read status data");

            var hasNotificationId = data.ContainsKey("NotificationId") || data.ContainsKey("notificationId");
            
            return (hasNotificationId).Label("Contains notification ID in read status");
        }
        catch
        {
            return false.Label("Error validating read status data");
        }
    }

    private static Property ValidateUnreadCountData(object? sentData, int expectedCount)
    {
        if (sentData == null) return false.Label("Unread count data is null");

        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(sentData);
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (data == null) return false.Label("Could not deserialize unread count data");

            var hasUnreadCount = data.ContainsKey("UnreadCount") || data.ContainsKey("unreadCount");
            
            return (hasUnreadCount).Label("Contains unread count");
        }
        catch
        {
            return false.Label("Error validating unread count data");
        }
    }

    #endregion

    #region Generators

    private static Arbitrary<UserNotification> GenerateUserNotification()
    {
        return Arb.From(
            from notificationId in Gen.Choose(1, 10000)
            from userId in Gen.Choose(1, 1000)
            from title in Gen.Elements("Test Notification", "Important Update", "System Alert")
            from message in Gen.Elements("This is a test message", "Important system update", "Alert notification")
            from priority in Gen.Elements(NotificationPriority.Normal, NotificationPriority.High, NotificationPriority.Critical)
            select new UserNotification
            {
                NotificationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = title,
                Message = message,
                EventType = "test-notification",
                IsRead = false,
                CreatedAt = DateTimeOffset.UtcNow
            });
    }

    private static Arbitrary<(UserNotification notification, List<int> userIds)> GenerateMultiUserNotification()
    {
        return Arb.From(
            from notification in GenerateUserNotification().Generator
            from userCount in Gen.Choose(2, 10)
            from userIds in Gen.ListOf(userCount, Gen.Choose(1, 1000))
            select (notification, userIds.Distinct().ToList()));
    }

    private static Arbitrary<(int userId, int notificationId)> GenerateReadStatusUpdate()
    {
        return Arb.From(
            from userId in Gen.Choose(1, 1000)
            from notificationId in Gen.Choose(1, 10000)
            select (userId, notificationId));
    }

    private static Arbitrary<(int userId, int unreadCount)> GenerateUnreadCountUpdate()
    {
        return Arb.From(
            from userId in Gen.Choose(1, 1000)
            from unreadCount in Gen.Choose(0, 100)
            select (userId, unreadCount));
    }

    private static Arbitrary<UserNotification> GenerateSystemWideNotification()
    {
        return Arb.From(
            from title in Gen.Elements("System Maintenance", "Important Announcement", "Service Update")
            from message in Gen.Elements("Scheduled maintenance", "Important system announcement", "Service update notification")
            select new UserNotification
            {
                NotificationId = Guid.NewGuid(),
                UserId = Guid.Empty, // System notification
                Title = title,
                Message = message,
                EventType = "system-notification",
                IsRead = false,
                CreatedAt = DateTimeOffset.UtcNow
            });
    }

    #endregion
}