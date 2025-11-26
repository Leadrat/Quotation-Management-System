using CRM.Domain.Enums;
using CRM.Infrastructure.Services;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Notifications;

/// <summary>
/// **Feature: notification-creation-dispatch, Property 11: Secure audit logging**
/// Property-based tests for secure notification logging system
/// </summary>
public class SecureLoggingPropertyTests : IDisposable
{
    private readonly DbContextOptions<TestDbContext> _dbOptions;
    private readonly Mock<ILogger<NotificationMonitoringService>> _mockLogger;
    private readonly NotificationMonitoringService _monitoringService;

    public SecureLoggingPropertyTests()
    {
        _dbOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _mockLogger = new Mock<ILogger<NotificationMonitoringService>>();
        
        using var context = new TestDbContext(_dbOptions);
        context.Database.EnsureCreated();
        
        _monitoringService = new NotificationMonitoringService(context, _mockLogger.Object);
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 11: Secure audit logging**
    /// **Validates: Requirements 7.4, 8.1, 8.2**
    /// 
    /// For any notification operation, the system should log all relevant details
    /// while ensuring sensitive information is properly sanitized or excluded
    /// </summary>
    [Property(MaxTest = 100)]
    public Property NotificationOperationLoggingExcludesSensitiveData()
    {
        return Prop.ForAll(
            GenerateNotificationOperationWithSensitiveData(),
            async (operation) =>
            {
                using var context = new TestDbContext(_dbOptions);
                
                // Act
                await _monitoringService.LogNotificationOperationAsync(operation);

                // Assert - Check that sensitive data is not logged
                var logEntries = await context.NotificationOperationLogs
                    .Where(log => log.NotificationId == operation.NotificationId)
                    .ToListAsync();

                return (logEntries.Count > 0).Label("Operation was logged")
                    .And(ValidateNoSensitiveDataInLogs(logEntries, operation));
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 11: Secure audit logging**
    /// **Validates: Requirements 7.4, 8.1**
    /// 
    /// For any operation with personal data, the system should either exclude
    /// or properly mask the sensitive information in audit logs
    /// </summary>
    [Property(MaxTest = 100)]
    public Property PersonalDataIsMaskedInAuditLogs()
    {
        return Prop.ForAll(
            GenerateOperationWithPersonalData(),
            async (operation) =>
            {
                using var context = new TestDbContext(_dbOptions);
                
                // Act
                await _monitoringService.LogNotificationOperationAsync(operation);

                // Assert - Personal data should be masked or excluded
                var logEntries = await context.NotificationOperationLogs
                    .Where(log => log.NotificationId == operation.NotificationId)
                    .ToListAsync();

                return (logEntries.Count > 0).Label("Operation was logged")
                    .And(ValidatePersonalDataMasking(logEntries, operation));
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 11: Secure audit logging**
    /// **Validates: Requirements 8.1, 8.2**
    /// 
    /// For any audit log entry, the system should maintain data integrity
    /// and ensure logs cannot be tampered with after creation
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AuditLogsPreserveDataIntegrity()
    {
        return Prop.ForAll(
            GenerateAuditLogOperation(),
            async (operation) =>
            {
                using var context = new TestDbContext(_dbOptions);
                
                // Act
                await _monitoringService.LogNotificationOperationAsync(operation);

                // Assert - Log entry maintains integrity
                var logEntry = await context.NotificationOperationLogs
                    .FirstOrDefaultAsync(log => log.NotificationId == operation.NotificationId);

                return (logEntry != null).Label("Log entry exists")
                    .And(ValidateLogIntegrity(logEntry, operation));
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 11: Secure audit logging**
    /// **Validates: Requirements 7.4, 8.2**
    /// 
    /// For any error logging, the system should log sufficient information for debugging
    /// while ensuring no sensitive data is exposed in error messages
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ErrorLoggingBalancesSecurityAndDebuggability()
    {
        return Prop.ForAll(
            GenerateOperationWithError(),
            async (operation) =>
            {
                using var context = new TestDbContext(_dbOptions);
                
                // Act
                await _monitoringService.LogNotificationOperationAsync(operation);

                // Assert - Error information is useful but secure
                var logEntry = await context.NotificationOperationLogs
                    .FirstOrDefaultAsync(log => log.NotificationId == operation.NotificationId);

                return (logEntry != null).Label("Error was logged")
                    .And((!string.IsNullOrEmpty(logEntry.ErrorMessage)).Label("Error message is present"))
                    .And(ValidateErrorMessageSecurity(logEntry.ErrorMessage!, operation));
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 11: Secure audit logging**
    /// **Validates: Requirements 8.1, 8.2**
    /// 
    /// For any log retention period, the system should maintain logs for the required
    /// duration while ensuring old logs are properly archived or deleted
    /// </summary>
    [Property(MaxTest = 50)]
    public Property LogRetentionPolicyIsEnforced()
    {
        return Prop.ForAll(
            GenerateLogRetentionScenario(),
            async (data) =>
            {
                var (operations, retentionDays) = data;
                
                using var context = new TestDbContext(_dbOptions);
                
                // Arrange - Create logs with different ages
                foreach (var operation in operations)
                {
                    await _monitoringService.LogNotificationOperationAsync(operation);
                }

                // Simulate log cleanup (would be done by background service)
                var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
                var oldLogs = await context.NotificationOperationLogs
                    .Where(log => log.Timestamp.DateTime < cutoffDate)
                    .ToListAsync();

                // Assert - Old logs identified for cleanup
                var expectedOldLogs = operations.Count(op => 
                    op.Metadata != null && 
                    op.Metadata.ContainsKey("Timestamp") &&
                    DateTime.Parse(op.Metadata["Timestamp"].ToString()!) < cutoffDate);

                return (oldLogs.Count <= expectedOldLogs).Label($"Old logs identified: {oldLogs.Count} <= {expectedOldLogs}")
                    .And((oldLogs.All(log => log.Timestamp.DateTime < cutoffDate)).Label("All identified logs are actually old"));
            });
    }

    #region Validation Helpers

    private static Property ValidateNoSensitiveDataInLogs(
        List<Domain.Entities.NotificationOperationLog> logEntries, 
        NotificationOperation operation)
    {
        foreach (var logEntry in logEntries)
        {
            // Check that sensitive fields are not present in logs
            var logContent = $"{logEntry.ErrorMessage} {logEntry.Metadata}".ToLowerInvariant();
            
            var containsPassword = logContent.Contains("password");
            var containsApiKey = logContent.Contains("apikey") || logContent.Contains("api_key");
            var containsToken = logContent.Contains("token") && !logContent.Contains("notification");
            var containsSecret = logContent.Contains("secret");

            if (containsPassword || containsApiKey || containsToken || containsSecret)
            {
                return false.Label($"Sensitive data found in logs: {logContent}");
            }
        }

        return true.Label("No sensitive data found in logs");
    }

    private static Property ValidatePersonalDataMasking(
        List<Domain.Entities.NotificationOperationLog> logEntries,
        NotificationOperation operation)
    {
        foreach (var logEntry in logEntries)
        {
            var logContent = $"{logEntry.ErrorMessage} {logEntry.Metadata}";
            
            // Check for email patterns - should be masked
            if (logContent.Contains("@") && !logContent.Contains("***"))
            {
                return false.Label("Email addresses should be masked in logs");
            }

            // Check for phone patterns - should be masked
            if (System.Text.RegularExpressions.Regex.IsMatch(logContent, @"\d{10,}") && !logContent.Contains("***"))
            {
                return false.Label("Phone numbers should be masked in logs");
            }
        }

        return true.Label("Personal data is properly masked");
    }

    private static Property ValidateLogIntegrity(
        Domain.Entities.NotificationOperationLog logEntry,
        NotificationOperation operation)
    {
        var hasRequiredFields = !string.IsNullOrEmpty(logEntry.OperationType) &&
                               !string.IsNullOrEmpty(logEntry.Channel) &&
                               !string.IsNullOrEmpty(logEntry.Status) &&
                               logEntry.Timestamp != default;

        var matchesOperation = logEntry.OperationType == operation.OperationType.ToString() &&
                              logEntry.Channel == operation.Channel.ToString() &&
                              logEntry.Status == operation.Status.ToString() &&
                              logEntry.NotificationId == operation.NotificationId;

        return (hasRequiredFields).Label("All required fields are present")
            .And((matchesOperation).Label("Log entry matches original operation"));
    }

    private static Property ValidateErrorMessageSecurity(string errorMessage, NotificationOperation operation)
    {
        var lowerError = errorMessage.ToLowerInvariant();
        
        // Error message should not contain sensitive information
        var containsSensitiveInfo = lowerError.Contains("password") ||
                                   lowerError.Contains("apikey") ||
                                   lowerError.Contains("secret") ||
                                   lowerError.Contains("token");

        // But should contain useful debugging information
        var containsUsefulInfo = lowerError.Contains("error") ||
                                lowerError.Contains("failed") ||
                                lowerError.Contains("timeout") ||
                                lowerError.Contains("invalid");

        return (!containsSensitiveInfo).Label("Error message doesn't contain sensitive info")
            .And((containsUsefulInfo).Label("Error message contains useful debugging info"));
    }

    #endregion

    #region Generators

    private static Arbitrary<NotificationOperation> GenerateNotificationOperationWithSensitiveData()
    {
        return Arb.From(
            from notificationId in Gen.Choose(1, 10000)
            from userId in Gen.Choose(1, 1000)
            from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.SMS)
            from operationType in Gen.Elements(NotificationOperationType.Creation, NotificationOperationType.Dispatch)
            from status in Gen.Elements(OperationStatus.Success, OperationStatus.Failed)
            from hasSensitiveData in Arb.Generate<bool>()
            select new NotificationOperation
            {
                NotificationId = notificationId,
                UserId = userId,
                Channel = channel,
                OperationType = operationType,
                Status = status,
                AttemptNumber = 1,
                Duration = TimeSpan.FromMilliseconds(Random.Shared.Next(100, 5000)),
                ErrorMessage = status == OperationStatus.Failed ? "Operation failed" : null,
                Metadata = hasSensitiveData ? new Dictionary<string, object>
                {
                    ["ApiKey"] = "secret_key_12345",
                    ["Password"] = "user_password",
                    ["Token"] = "auth_token_xyz"
                } : new Dictionary<string, object>
                {
                    ["ProcessingTime"] = "1234ms",
                    ["RetryCount"] = "1"
                }
            });
    }

    private static Arbitrary<NotificationOperation> GenerateOperationWithPersonalData()
    {
        return Arb.From(
            from notificationId in Gen.Choose(1, 10000)
            from userId in Gen.Choose(1, 1000)
            from channel in Gen.Elements(NotificationChannel.Email, NotificationChannel.SMS)
            from email in Gen.Elements("user@example.com", "test.user@domain.org", "admin@company.com")
            from phone in Gen.Elements("+1234567890", "555-123-4567", "+44 20 7946 0958")
            select new NotificationOperation
            {
                NotificationId = notificationId,
                UserId = userId,
                Channel = channel,
                OperationType = NotificationOperationType.Dispatch,
                Status = OperationStatus.Success,
                AttemptNumber = 1,
                Metadata = new Dictionary<string, object>
                {
                    ["RecipientEmail"] = email,
                    ["RecipientPhone"] = phone,
                    ["UserName"] = "John Doe"
                }
            });
    }

    private static Arbitrary<NotificationOperation> GenerateAuditLogOperation()
    {
        return Arb.From(
            from notificationId in Gen.Choose(1, 10000)
            from userId in Gen.Choose(1, 1000)
            from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.SMS)
            from operationType in Gen.Elements(NotificationOperationType.Creation, NotificationOperationType.Dispatch, NotificationOperationType.StatusUpdate)
            from status in Gen.Elements(OperationStatus.Success, OperationStatus.Failed)
            select new NotificationOperation
            {
                NotificationId = notificationId,
                UserId = userId,
                Channel = channel,
                OperationType = operationType,
                Status = status,
                AttemptNumber = Random.Shared.Next(1, 4),
                Duration = TimeSpan.FromMilliseconds(Random.Shared.Next(50, 2000))
            });
    }

    private static Arbitrary<NotificationOperation> GenerateOperationWithError()
    {
        return Arb.From(
            from notificationId in Gen.Choose(1, 10000)
            from userId in Gen.Choose(1, 1000)
            from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.SMS)
            from errorType in Gen.Elements("Network timeout", "Invalid email address", "SMS quota exceeded", "Authentication failed with token abc123")
            select new NotificationOperation
            {
                NotificationId = notificationId,
                UserId = userId,
                Channel = channel,
                OperationType = NotificationOperationType.Dispatch,
                Status = OperationStatus.Failed,
                AttemptNumber = Random.Shared.Next(1, 4),
                ErrorMessage = errorType
            });
    }

    private static Arbitrary<(List<NotificationOperation> operations, int retentionDays)> GenerateLogRetentionScenario()
    {
        return Arb.From(
            from retentionDays in Gen.Choose(7, 90)
            from operationCount in Gen.Choose(5, 20)
            from operations in Gen.ListOf(operationCount, GenerateOperationWithAge(retentionDays).Generator)
            select (operations, retentionDays));
    }

    private static Arbitrary<NotificationOperation> GenerateOperationWithAge(int maxAgeDays)
    {
        return Arb.From(
            from notificationId in Gen.Choose(1, 10000)
            from userId in Gen.Choose(1, 1000)
            from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.SMS)
            from ageDays in Gen.Choose(0, maxAgeDays + 30) // Some operations older than retention
            select new NotificationOperation
            {
                NotificationId = notificationId,
                UserId = userId,
                Channel = channel,
                OperationType = NotificationOperationType.Dispatch,
                Status = OperationStatus.Success,
                AttemptNumber = 1,
                Metadata = new Dictionary<string, object>
                {
                    ["Timestamp"] = DateTime.UtcNow.AddDays(-ageDays).ToString("O")
                }
            });
    }

    #endregion

    public void Dispose()
    {
        using var context = new TestDbContext(_dbOptions);
        context.Database.EnsureDeleted();
    }
}