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
/// **Feature: notification-creation-dispatch, Property 6: Comprehensive dispatch logging**
/// Property-based tests for notification dispatch logging system
/// </summary>
public class NotificationDispatchPropertyTests : IDisposable
{
    private readonly DbContextOptions<TestDbContext> _dbOptions;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<NotificationDispatchService>> _mockLogger;
    private readonly Mock<IRealTimeNotificationService> _mockRealTimeService;
    private readonly Mock<IEmailNotificationService> _mockEmailService;
    private readonly Mock<ISmsService> _mockSmsService;

    public NotificationDispatchPropertyTests()
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
    /// **Feature: notification-creation-dispatch, Property 6: Comprehensive dispatch logging**
    /// **Validates: Requirements 4.1, 4.2, 4.3**
    /// 
    /// For any notification dispatch attempt, the system should create a complete audit log
    /// with all relevant details including timestamps, status, and error information
    /// </summary>
    [Property(MaxTest = 100)]
    public Property DispatchAttemptsCreateComprehensiveAuditLogs()
    {
        return Prop.ForAll(
            GenerateNotificationAndChannel(),
            async (data) =>
            {
                var (notification, channel) = data;
                
                using var context = new TestDbContext(_dbOptions);
                await context.Database.EnsureCreatedAsync();
                
                // Arrange
                context.Users.Add(new User { Id = notification.UserId, Email = "test@example.com", PhoneNumber = "+1234567890", IsActive = true });
                context.UserNotifications.Add(notification);
                await context.SaveChangesAsync();

                var dispatchService = new NotificationDispatchService(context, _mockServiceProvider.Object, _mockLogger.Object);

                // Act
                await dispatchService.DispatchNotificationAsync(notification.Id, channel);

                // Assert - Comprehensive audit log created
                var dispatchAttempts = await context.NotificationDispatchAttempts
                    .Where(da => da.NotificationId == notification.Id)
                    .ToListAsync();

                return (dispatchAttempts.Count > 0).Label("Dispatch attempt logged")
                    .And((dispatchAttempts.All(da => da.AttemptedAt != default)).Label("Attempt timestamp recorded"))
                    .And((dispatchAttempts.All(da => da.Channel == channel)).Label("Channel recorded correctly"))
                    .And((dispatchAttempts.All(da => da.Status != DispatchStatus.Unknown)).Label("Status recorded"))
                    .And((dispatchAttempts.All(da => da.AttemptNumber > 0)).Label("Attempt number recorded"));
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 6: Comprehensive dispatch logging**
    /// **Validates: Requirements 4.2, 4.3**
    /// 
    /// For any failed dispatch attempt, the system should log detailed error information
    /// including error messages and technical details for troubleshooting
    /// </summary>
    [Property(MaxTest = 100)]
    public Property FailedDispatchAttemptsLogDetailedErrorInformation()
    {
        return Prop.ForAll(
            GenerateNotificationForFailedDispatch(),
            async (notification) =>
            {
                using var context = new TestDbContext(_dbOptions);
                await context.Database.EnsureCreatedAsync();
                
                // Arrange - Setup for failure
                context.Users.Add(new User { Id = notification.UserId, Email = "", PhoneNumber = "", IsActive = true }); // Invalid contact info
                context.UserNotifications.Add(notification);
                await context.SaveChangesAsync();

                var dispatchService = new NotificationDispatchService(context, _mockServiceProvider.Object, _mockLogger.Object);

                // Act
                await dispatchService.DispatchNotificationAsync(notification.Id, NotificationChannel.Email);

                // Assert - Failed attempts log error details
                var failedAttempts = await context.NotificationDispatchAttempts
                    .Where(da => da.NotificationId == notification.Id && da.Status == DispatchStatus.Failed)
                    .ToListAsync();

                return (failedAttempts.Count > 0).Label("Failed attempt logged")
                    .And((failedAttempts.All(da => !string.IsNullOrEmpty(da.ErrorMessage))).Label("Error message recorded"))
                    .And((failedAttempts.All(da => da.AttemptedAt != default)).Label("Failure timestamp recorded"))
                    .And((failedAttempts.All(da => da.DeliveredAt == null)).Label("No delivery timestamp for failed attempts"));
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 6: Comprehensive dispatch logging**
    /// **Validates: Requirements 4.1, 4.4**
    /// 
    /// For any successful dispatch attempt, the system should log delivery confirmation
    /// with external tracking IDs and delivery timestamps
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SuccessfulDispatchAttemptsLogDeliveryConfirmation()
    {
        return Prop.ForAll(
            GenerateNotificationForSuccessfulDispatch(),
            async (notification) =>
            {
                using var context = new TestDbContext(_dbOptions);
                await context.Database.EnsureCreatedAsync();
                
                // Arrange - Setup for success
                context.Users.Add(new User { Id = notification.UserId, Email = "valid@example.com", PhoneNumber = "+1234567890", IsActive = true });
                context.UserNotifications.Add(notification);
                await context.SaveChangesAsync();

                // Setup successful email response
                _mockEmailService.Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new EmailResult { IsSuccess = true, MessageId = "email_123" });

                var dispatchService = new NotificationDispatchService(context, _mockServiceProvider.Object, _mockLogger.Object);

                // Act
                await dispatchService.DispatchNotificationAsync(notification.Id, NotificationChannel.Email);

                // Assert - Successful attempts log delivery details
                var successfulAttempts = await context.NotificationDispatchAttempts
                    .Where(da => da.NotificationId == notification.Id && da.Status == DispatchStatus.Delivered)
                    .ToListAsync();

                return (successfulAttempts.Count > 0).Label("Successful attempt logged")
                    .And((successfulAttempts.All(da => da.DeliveredAt.HasValue)).Label("Delivery timestamp recorded"))
                    .And((successfulAttempts.All(da => !string.IsNullOrEmpty(da.ExternalId))).Label("External tracking ID recorded"))
                    .And((successfulAttempts.All(da => string.IsNullOrEmpty(da.ErrorMessage))).Label("No error message for successful attempts"));
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 6: Comprehensive dispatch logging**
    /// **Validates: Requirements 4.3, 4.4**
    /// 
    /// For any retry attempt, the system should maintain the complete chain of attempts
    /// with proper sequencing and reference to previous attempts
    /// </summary>
    [Property(MaxTest = 50)]
    public Property RetryAttemptsPreserveCompleteAuditChain()
    {
        return Prop.ForAll(
            GenerateNotificationForRetry(),
            async (notification) =>
            {
                using var context = new TestDbContext(_dbOptions);
                await context.Database.EnsureCreatedAsync();
                
                // Arrange
                context.Users.Add(new User { Id = notification.UserId, Email = "test@example.com", PhoneNumber = "+1234567890", IsActive = true });
                context.UserNotifications.Add(notification);
                
                // Create initial failed attempt
                var initialAttempt = new NotificationDispatchAttempt
                {
                    NotificationId = notification.Id,
                    Channel = NotificationChannel.Email,
                    Status = DispatchStatus.Failed,
                    AttemptedAt = DateTime.UtcNow.AddMinutes(-5),
                    AttemptNumber = 1,
                    ErrorMessage = "Initial failure"
                };
                context.NotificationDispatchAttempts.Add(initialAttempt);
                await context.SaveChangesAsync();

                var dispatchService = new NotificationDispatchService(context, _mockServiceProvider.Object, _mockLogger.Object);

                // Act - Retry the failed attempt
                await dispatchService.RetryFailedDispatchAsync(initialAttempt.Id);

                // Assert - Complete audit chain preserved
                var allAttempts = await context.NotificationDispatchAttempts
                    .Where(da => da.NotificationId == notification.Id)
                    .OrderBy(da => da.AttemptNumber)
                    .ToListAsync();

                return (allAttempts.Count >= 2).Label("Multiple attempts logged")
                    .And((allAttempts.Select(a => a.AttemptNumber).SequenceEqual(Enumerable.Range(1, allAttempts.Count))).Label("Attempt numbers sequential"))
                    .And((allAttempts.Skip(1).All(a => a.PreviousAttemptId.HasValue)).Label("Retry attempts reference previous attempts"))
                    .And((allAttempts.All(a => a.AttemptedAt != default)).Label("All attempts have timestamps"));
            });
    }

    #region Setup and Generators

    private void SetupMockServices()
    {
        // Setup InApp service
        var mockInAppService = new Mock<InAppNotificationDispatchService>(
            Mock.Of<IAppDbContext>(), _mockRealTimeService.Object, Mock.Of<ILogger<InAppNotificationDispatchService>>());
        
        // Setup Email service
        var mockEmailDispatchService = new Mock<EmailNotificationDispatchService>(
            Mock.Of<IAppDbContext>(), _mockEmailService.Object, Mock.Of<ILogger<EmailNotificationDispatchService>>());
        
        // Setup SMS service
        var mockSmsDispatchService = new Mock<SmsNotificationDispatchService>(
            Mock.Of<IAppDbContext>(), _mockSmsService.Object, Mock.Of<ILogger<SmsNotificationDispatchService>>());

        _mockServiceProvider.Setup(sp => sp.GetService(typeof(InAppNotificationDispatchService)))
            .Returns(mockInAppService.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(EmailNotificationDispatchService)))
            .Returns(mockEmailDispatchService.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(SmsNotificationDispatchService)))
            .Returns(mockSmsDispatchService.Object);
    }

    private static Arbitrary<(UserNotification notification, NotificationChannel channel)> GenerateNotificationAndChannel()
    {
        return Arb.From(
            from notificationId in Gen.Choose(1, 10000)
            from userId in Gen.Choose(1, 1000)
            from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.SMS)
            from title in Gen.Elements("Test Notification", "Important Update", "System Alert")
            from message in Gen.Elements("This is a test message", "Important system update", "Alert notification")
            select (new UserNotification
            {
                Id = notificationId,
                UserId = userId,
                Title = title,
                Message = message,
                Type = NotificationEventType.General,
                Priority = NotificationPriority.Normal,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }, channel));
    }

    private static Arbitrary<UserNotification> GenerateNotificationForFailedDispatch()
    {
        return Arb.From(
            from notificationId in Gen.Choose(1, 10000)
            from userId in Gen.Choose(1, 1000)
            select new UserNotification
            {
                Id = notificationId,
                UserId = userId,
                Title = "Test Notification",
                Message = "This will fail to dispatch",
                Type = NotificationEventType.General,
                Priority = NotificationPriority.Normal,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
    }

    private static Arbitrary<UserNotification> GenerateNotificationForSuccessfulDispatch()
    {
        return Arb.From(
            from notificationId in Gen.Choose(1, 10000)
            from userId in Gen.Choose(1, 1000)
            select new UserNotification
            {
                Id = notificationId,
                UserId = userId,
                Title = "Success Notification",
                Message = "This will dispatch successfully",
                Type = NotificationEventType.General,
                Priority = NotificationPriority.Normal,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
    }

    private static Arbitrary<UserNotification> GenerateNotificationForRetry()
    {
        return Arb.From(
            from notificationId in Gen.Choose(1, 10000)
            from userId in Gen.Choose(1, 1000)
            select new UserNotification
            {
                Id = notificationId,
                UserId = userId,
                Title = "Retry Notification",
                Message = "This will be retried",
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

/// <summary>
/// Test DbContext for property tests
/// </summary>
public class TestDbContext : DbContext, IAppDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserNotification> UserNotifications { get; set; } = null!;
    public DbSet<NotificationDispatchAttempt> NotificationDispatchAttempts { get; set; } = null!;

    // Other required DbSets for IAppDbContext (simplified for testing)
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Quotation> Quotations { get; set; } = null!;
    public DbSet<QuotationItem> QuotationItems { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Country> Countries { get; set; } = null!;
    public DbSet<CompanyDetail> CompanyDetails { get; set; } = null!;
    public DbSet<CountryCompanyIdentifier> CountryCompanyIdentifiers { get; set; } = null!;
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; } = null!;
    public DbSet<EmailNotificationLog> EmailNotificationLogs { get; set; } = null!;
    public DbSet<NotificationEventType> NotificationEventTypes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Simplified configuration for testing
        modelBuilder.Entity<User>().HasKey(u => u.Id);
        modelBuilder.Entity<UserNotification>().HasKey(n => n.Id);
        modelBuilder.Entity<NotificationDispatchAttempt>().HasKey(da => da.Id);
        
        base.OnModelCreating(modelBuilder);
    }
}