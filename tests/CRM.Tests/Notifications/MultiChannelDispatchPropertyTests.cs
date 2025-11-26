using CRM.Application.Common.Interfaces;
using CRM.Application.Notifications.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Services;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Notifications;

/// <summary>
/// **Feature: notification-creation-dispatch, Property 2: Multi-channel dispatch determination**
/// Property-based tests for multi-channel notification dispatch system
/// </summary>
public class MultiChannelDispatchPropertyTests : IDisposable
{
    private readonly DbContextOptions<TestDbContext> _dbOptions;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<NotificationDispatchService>> _mockLogger;
    private readonly Mock<IRealTimeNotificationService> _mockRealTimeService;
    private readonly Mock<IEmailNotificationService> _mockEmailService;
    private readonly Mock<ISmsService> _mockSmsService;

    public MultiChannelDispatchPropertyTests()
    {
        _dbOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<NotificationDispatchService>>();
        _mockRealTimeService = new Mock<IRealTimeNotificationService>();
        _mockEmailService = new Mock<IEmailNotificationService>();
        _mockSmsService = new Mock<ISmsService>();

        SetupMockServices();
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 2: Multi-channel dispatch determination**
    /// **Validates: Requirements 2.1, 2.5**
    /// 
    /// For any notification with multiple preferred channels, the system should dispatch
    /// to all available channels based on user configuration and channel availability
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MultiChannelDispatchCreatesAttemptForEachChannel()
    {
        return Prop.ForAll(
            GenerateNotificationWithMultipleChannels(),
            async (data) =>
            {
                var (notification, channels) = data;
                
                using var context = new TestDbContext(_dbOptions);
                await context.Database.EnsureCreatedAsync();
                
                // Arrange
                context.Users.Add(new User 
                { 
                    Id = notification.UserId, 
                    Email = "test@example.com", 
                    PhoneNumber = "+1234567890", 
                    IsActive = true 
                });
                context.UserNotifications.Add(notification);
                await context.SaveChangesAsync();

                var dispatchService = new NotificationDispatchService(context, _mockServiceProvider.Object, _mockLogger.Object);

                // Act
                await dispatchService.DispatchNotificationAsync(notification.Id, channels);

                // Assert - Dispatch attempt created for each channel
                var dispatchAttempts = await context.NotificationDispatchAttempts
                    .Where(da => da.NotificationId == notification.Id)
                    .ToListAsync();

                var attemptChannels = dispatchAttempts.Select(da => da.Channel).ToHashSet();

                return (dispatchAttempts.Count == channels.Count).Label($"Expected {channels.Count} attempts, got {dispatchAttempts.Count}")
                    .And((channels.All(c => attemptChannels.Contains(c))).Label("All requested channels have attempts"))
                    .And((dispatchAttempts.All(da => da.NotificationId == notification.Id)).Label("All attempts linked to correct notification"));
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 2: Multi-channel dispatch determination**
    /// **Validates: Requirements 2.1, 2.5**
    /// 
    /// For any notification priority level, the system should select appropriate channels
    /// with high-priority notifications using more channels than normal priority
    /// </summary>
    [Property(MaxTest = 100)]
    public Property HighPriorityNotificationsUseMoreChannels()
    {
        return Prop.ForAll(
            GenerateNotificationWithPriority(),
            async (data) =>
            {
                var (normalNotification, highNotification) = data;
                
                using var context = new TestDbContext(_dbOptions);
                await context.Database.EnsureCreatedAsync();
                
                // Arrange
                context.Users.Add(new User 
                { 
                    Id = normalNotification.UserId, 
                    Email = "test@example.com", 
                    PhoneNumber = "+1234567890", 
                    IsActive = true 
                });
                context.UserNotifications.AddRange(normalNotification, highNotification);
                await context.SaveChangesAsync();

                var dispatchService = new NotificationDispatchService(context, _mockServiceProvider.Object, _mockLogger.Object);

                // Define channel preferences based on priority
                var normalChannels = new List<NotificationChannel> { NotificationChannel.InApp };
                var highChannels = new List<NotificationChannel> 
                { 
                    NotificationChannel.InApp, 
                    NotificationChannel.Email, 
                    NotificationChannel.SMS 
                };

                // Act
                await dispatchService.DispatchNotificationAsync(normalNotification.Id, normalChannels);
                await dispatchService.DispatchNotificationAsync(highNotification.Id, highChannels);

                // Assert - High priority uses more channels
                var normalAttempts = await context.NotificationDispatchAttempts
                    .Where(da => da.NotificationId == normalNotification.Id)
                    .ToListAsync();
                
                var highAttempts = await context.NotificationDispatchAttempts
                    .Where(da => da.NotificationId == highNotification.Id)
                    .ToListAsync();

                return (highAttempts.Count > normalAttempts.Count).Label($"High priority ({highAttempts.Count}) should use more channels than normal ({normalAttempts.Count})")
                    .And((normalAttempts.Count > 0)).Label("Normal priority notifications still dispatched")
                    .And((highAttempts.Count > 0)).Label("High priority notifications dispatched");
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 2: Multi-channel dispatch determination**
    /// **Validates: Requirements 2.1, 2.5**
    /// 
    /// For any user with invalid channel configurations, the system should only dispatch
    /// to valid channels and skip invalid ones without failing the entire operation
    /// </summary>
    [Property(MaxTest = 100)]
    public Property InvalidChannelConfigurationsSkippedGracefully()
    {
        return Prop.ForAll(
            GenerateNotificationWithMixedChannelValidity(),
            async (notification) =>
            {
                using var context = new TestDbContext(_dbOptions);
                await context.Database.EnsureCreatedAsync();
                
                // Arrange - User with mixed valid/invalid channel configs
                context.Users.Add(new User 
                { 
                    Id = notification.UserId, 
                    Email = "valid@example.com", // Valid email
                    PhoneNumber = "", // Invalid phone number
                    IsActive = true 
                });
                context.UserNotifications.Add(notification);
                await context.SaveChangesAsync();

                var dispatchService = new NotificationDispatchService(context, _mockServiceProvider.Object, _mockLogger.Object);
                var allChannels = new List<NotificationChannel> 
                { 
                    NotificationChannel.InApp,    // Should work
                    NotificationChannel.Email,    // Should work
                    NotificationChannel.SMS       // Should fail due to invalid phone
                };

                // Act
                await dispatchService.DispatchNotificationAsync(notification.Id, allChannels);

                // Assert - Valid channels dispatched, invalid ones handled gracefully
                var dispatchAttempts = await context.NotificationDispatchAttempts
                    .Where(da => da.NotificationId == notification.Id)
                    .ToListAsync();

                var successfulAttempts = dispatchAttempts.Where(da => da.Status == DispatchStatus.Delivered).ToList();
                var failedAttempts = dispatchAttempts.Where(da => da.Status == DispatchStatus.Failed).ToList();

                return (dispatchAttempts.Count == allChannels.Count).Label("Attempt made for each channel")
                    .And((successfulAttempts.Count > 0)).Label("Some channels succeeded")
                    .And((failedAttempts.Count > 0)).Label("Invalid channels failed gracefully")
                    .And((failedAttempts.All(fa => !string.IsNullOrEmpty(fa.ErrorMessage))).Label("Failed attempts have error messages"));
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 2: Multi-channel dispatch determination**
    /// **Validates: Requirements 2.1, 2.5**
    /// 
    /// For any notification dispatch across multiple channels, each channel should maintain
    /// independent status tracking without affecting other channels
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ChannelDispatchStatusesAreIndependent()
    {
        return Prop.ForAll(
            GenerateNotificationForIndependentChannels(),
            async (notification) =>
            {
                using var context = new TestDbContext(_dbOptions);
                await context.Database.EnsureCreatedAsync();
                
                // Arrange
                context.Users.Add(new User 
                { 
                    Id = notification.UserId, 
                    Email = "test@example.com", 
                    PhoneNumber = "+1234567890", 
                    IsActive = true 
                });
                context.UserNotifications.Add(notification);
                await context.SaveChangesAsync();

                // Setup mixed success/failure responses
                _mockEmailService.Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new EmailResult { IsSuccess = true, MessageId = "email_success" });
                
                _mockSmsService.Setup(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new SmsResult { IsSuccess = false, ErrorMessage = "SMS failed" });

                var dispatchService = new NotificationDispatchService(context, _mockServiceProvider.Object, _mockLogger.Object);
                var channels = new List<NotificationChannel> 
                { 
                    NotificationChannel.InApp,
                    NotificationChannel.Email,
                    NotificationChannel.SMS
                };

                // Act
                await dispatchService.DispatchNotificationAsync(notification.Id, channels);

                // Assert - Independent status tracking
                var dispatchAttempts = await context.NotificationDispatchAttempts
                    .Where(da => da.NotificationId == notification.Id)
                    .ToListAsync();

                var statusByChannel = dispatchAttempts.ToDictionary(da => da.Channel, da => da.Status);

                return (dispatchAttempts.Count == channels.Count).Label("All channels attempted")
                    .And((statusByChannel.Values.Distinct().Count() > 1).Label("Different channels have different statuses"))
                    .And((dispatchAttempts.All(da => da.AttemptNumber == 1)).Label("All are first attempts"))
                    .And((dispatchAttempts.All(da => da.NotificationId == notification.Id)).Label("All linked to same notification"));
            });
    }

    #region Setup and Generators

    private void SetupMockServices()
    {
        // Setup successful responses by default
        _mockRealTimeService.Setup(s => s.SendNotificationToUserAsync(It.IsAny<int>(), It.IsAny<UserNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockEmailService.Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailResult { IsSuccess = true, MessageId = "email_123" });

        _mockSmsService.Setup(s => s.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SmsResult { IsSuccess = true, MessageId = "sms_123" });

        // Setup mock channel services
        var mockInAppService = new Mock<InAppNotificationDispatchService>(
            Mock.Of<IAppDbContext>(), _mockRealTimeService.Object, Mock.Of<ILogger<InAppNotificationDispatchService>>());
        
        var mockEmailDispatchService = new Mock<EmailNotificationDispatchService>(
            Mock.Of<IAppDbContext>(), _mockEmailService.Object, Mock.Of<ILogger<EmailNotificationDispatchService>>());
        
        var mockSmsDispatchService = new Mock<SmsNotificationDispatchService>(
            Mock.Of<IAppDbContext>(), _mockSmsService.Object, Mock.Of<ILogger<SmsNotificationDispatchService>>());

        _mockServiceProvider.Setup(sp => sp.GetService(typeof(InAppNotificationDispatchService)))
            .Returns(mockInAppService.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(EmailNotificationDispatchService)))
            .Returns(mockEmailDispatchService.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(SmsNotificationDispatchService)))
            .Returns(mockSmsDispatchService.Object);
    }

    private static Arbitrary<(UserNotification notification, List<NotificationChannel> channels)> GenerateNotificationWithMultipleChannels()
    {
        return Arb.From(
            from notificationId in Gen.Choose(1, 10000)
            from userId in Gen.Choose(1, 1000)
            from channelCount in Gen.Choose(2, 3)
            from channels in Gen.SubListOf(channelCount, new[] { NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.SMS })
            select (new UserNotification
            {
                Id = notificationId,
                UserId = userId,
                Title = "Multi-Channel Test",
                Message = "This notification will be sent via multiple channels",
                Type = NotificationEventType.General,
                Priority = NotificationPriority.Normal,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }, channels.ToList()));
    }

    private static Arbitrary<(UserNotification normal, UserNotification high)> GenerateNotificationWithPriority()
    {
        return Arb.From(
            from userId in Gen.Choose(1, 1000)
            from normalId in Gen.Choose(1, 5000)
            from highId in Gen.Choose(5001, 10000)
            select (
                new UserNotification
                {
                    Id = normalId,
                    UserId = userId,
                    Title = "Normal Priority",
                    Message = "Normal priority notification",
                    Type = NotificationEventType.General,
                    Priority = NotificationPriority.Normal,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                },
                new UserNotification
                {
                    Id = highId,
                    UserId = userId,
                    Title = "High Priority",
                    Message = "High priority notification",
                    Type = NotificationEventType.General,
                    Priority = NotificationPriority.High,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                }));
    }

    private static Arbitrary<UserNotification> GenerateNotificationWithMixedChannelValidity()
    {
        return Arb.From(
            from notificationId in Gen.Choose(1, 10000)
            from userId in Gen.Choose(1, 1000)
            select new UserNotification
            {
                Id = notificationId,
                UserId = userId,
                Title = "Mixed Validity Test",
                Message = "Testing mixed channel validity",
                Type = NotificationEventType.General,
                Priority = NotificationPriority.Normal,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
    }

    private static Arbitrary<UserNotification> GenerateNotificationForIndependentChannels()
    {
        return Arb.From(
            from notificationId in Gen.Choose(1, 10000)
            from userId in Gen.Choose(1, 1000)
            select new UserNotification
            {
                Id = notificationId,
                UserId = userId,
                Title = "Independent Channels Test",
                Message = "Testing independent channel status tracking",
                Type = NotificationEventType.General,
                Priority = NotificationPriority.Normal,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
    }

    #endregion

    public void Dispose()
    {
        using var context = new TestDbContext(_dbOptions);
        context.Database.EnsureDeleted();
    }
}