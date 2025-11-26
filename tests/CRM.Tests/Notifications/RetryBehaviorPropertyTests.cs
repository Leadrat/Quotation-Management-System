using CRM.Application.Common.Interfaces;
using CRM.Application.Notifications.Services;
using CRM.Domain.Enums;
using CRM.Infrastructure.Services;
using FsCheck;
using FsCheck.Xunit;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Notifications;

/// <summary>
/// **Feature: notification-creation-dispatch, Property 4: Retry with exponential backoff**
/// Property-based tests for notification retry behavior system
/// </summary>
public class RetryBehaviorPropertyTests
{
    private readonly Mock<IAppDbContext> _mockContext;
    private readonly Mock<IBackgroundJobClient> _mockBackgroundJobClient;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<RetryPolicyService>> _mockLogger;
    private readonly RetryPolicyService _retryPolicyService;

    public RetryBehaviorPropertyTests()
    {
        _mockContext = new Mock<IAppDbContext>();
        _mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<RetryPolicyService>>();

        SetupMockConfiguration();
        _retryPolicyService = new RetryPolicyService(
            _mockContext.Object,
            _mockBackgroundJobClient.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 4: Retry with exponential backoff**
    /// **Validates: Requirements 3.1, 3.2**
    /// 
    /// For any retry attempt number, the system should calculate delays using exponential backoff
    /// with each subsequent attempt having a longer delay than the previous one
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ExponentialBackoffIncreasesDelayWithEachAttempt()
    {
        return Prop.ForAll(
            GenerateRetryAttemptSequence(),
            (data) =>
            {
                var (channel, maxAttempts) = data;
                var delays = new List<TimeSpan>();

                // Calculate delays for sequential attempts
                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    var delay = _retryPolicyService.CalculateRetryDelay(attempt, channel);
                    delays.Add(delay);
                }

                // Verify exponential backoff pattern (allowing for jitter)
                var isIncreasing = true;
                for (int i = 1; i < delays.Count; i++)
                {
                    // Allow for some variance due to jitter, but overall trend should be increasing
                    var previousDelay = delays[i - 1];
                    var currentDelay = delays[i];
                    
                    // The delay should generally increase, but we allow up to 25% variance due to jitter
                    var minExpectedDelay = previousDelay.TotalMilliseconds * 0.75; // Account for negative jitter
                    
                    if (currentDelay.TotalMilliseconds < minExpectedDelay)
                    {
                        isIncreasing = false;
                        break;
                    }
                }

                return (delays.Count == maxAttempts).Label($"Generated {delays.Count} delays for {maxAttempts} attempts")
                    .And((delays.All(d => d > TimeSpan.Zero)).Label("All delays are positive"))
                    .And((delays.All(d => d <= TimeSpan.FromHours(1))).Label("All delays are within reasonable bounds"))
                    .And(isIncreasing.Label("Delays follow exponential backoff pattern (accounting for jitter)"));
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 4: Retry with exponential backoff**
    /// **Validates: Requirements 3.1, 3.2**
    /// 
    /// For any channel and attempt number, the system should respect maximum retry limits
    /// and not allow retries beyond the configured maximum
    /// </summary>
    [Property(MaxTest = 100)]
    public Property RetryLimitsAreRespected()
    {
        return Prop.ForAll(
            GenerateChannelAndAttemptNumber(),
            (data) =>
            {
                var (channel, attemptNumber) = data;
                var maxRetries = _retryPolicyService.GetMaxRetryAttempts(channel);
                var shouldRetry = _retryPolicyService.ShouldRetry(attemptNumber, channel);

                // Verify retry decision respects limits
                var expectedShouldRetry = attemptNumber < maxRetries;

                return (shouldRetry == expectedShouldRetry).Label($"Attempt {attemptNumber} with max {maxRetries}: expected {expectedShouldRetry}, got {shouldRetry}")
                    .And((maxRetries > 0).Label("Max retry attempts is positive"))
                    .And((maxRetries <= 10).Label("Max retry attempts is reasonable")); // Sanity check
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 4: Retry with exponential backoff**
    /// **Validates: Requirements 3.1, 3.2**
    /// 
    /// For any non-retryable error message, the system should not allow retries
    /// regardless of the attempt number being within limits
    /// </summary>
    [Property(MaxTest = 100)]
    public Property NonRetryableErrorsPreventRetries()
    {
        return Prop.ForAll(
            GenerateNonRetryableErrorScenario(),
            (data) =>
            {
                var (channel, attemptNumber, errorMessage, isNonRetryable) = data;
                var shouldRetry = _retryPolicyService.ShouldRetry(attemptNumber, channel, errorMessage);
                var maxRetries = _retryPolicyService.GetMaxRetryAttempts(channel);

                if (isNonRetryable)
                {
                    // Non-retryable errors should never allow retries
                    return (!shouldRetry).Label($"Non-retryable error '{errorMessage}' should prevent retry")
                        .And((attemptNumber < maxRetries).Label("Attempt number is within limits but still blocked"));
                }
                else
                {
                    // Retryable errors should follow normal retry logic
                    var expectedShouldRetry = attemptNumber < maxRetries;
                    return (shouldRetry == expectedShouldRetry).Label($"Retryable error should follow normal logic: expected {expectedShouldRetry}, got {shouldRetry}");
                }
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 4: Retry with exponential backoff**
    /// **Validates: Requirements 3.1, 3.2**
    /// 
    /// For any channel configuration, the calculated delays should include jitter
    /// to prevent thundering herd problems while maintaining the exponential pattern
    /// </summary>
    [Property(MaxTest = 100)]
    public Property JitterPreventsThunderingHerd()
    {
        return Prop.ForAll(
            GenerateJitterTestScenario(),
            (data) =>
            {
                var (channel, attemptNumber, iterations) = data;
                var delays = new List<TimeSpan>();

                // Calculate the same delay multiple times to observe jitter
                for (int i = 0; i < iterations; i++)
                {
                    var delay = _retryPolicyService.CalculateRetryDelay(attemptNumber, channel);
                    delays.Add(delay);
                }

                // Check for variance (jitter)
                var minDelay = delays.Min();
                var maxDelay = delays.Max();
                var hasVariance = maxDelay > minDelay;

                // All delays should be within reasonable bounds of each other (±25% jitter)
                var averageDelay = delays.Average(d => d.TotalMilliseconds);
                var allWithinBounds = delays.All(d => 
                {
                    var variance = Math.Abs(d.TotalMilliseconds - averageDelay) / averageDelay;
                    return variance <= 0.25; // 25% jitter tolerance
                });

                return (delays.Count == iterations).Label($"Generated {delays.Count} delays")
                    .And((delays.All(d => d > TimeSpan.Zero)).Label("All delays are positive"))
                    .And(hasVariance.Label("Delays show variance (jitter present)"))
                    .And(allWithinBounds.Label("All delays within jitter bounds (±25%)"));
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 4: Retry with exponential backoff**
    /// **Validates: Requirements 3.1, 3.2**
    /// 
    /// For any channel, the maximum delay cap should be respected
    /// even when exponential backoff would calculate a longer delay
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MaximumDelayCapIsRespected()
    {
        return Prop.ForAll(
            GenerateHighAttemptNumberScenario(),
            (data) =>
            {
                var (channel, highAttemptNumber) = data;
                var delay = _retryPolicyService.CalculateRetryDelay(highAttemptNumber, channel);

                // The delay should never exceed the maximum configured delay for the channel
                // We know from our configuration that max delay is 1 hour for email, 30 minutes for SMS, 10 minutes for InApp
                var expectedMaxDelay = channel switch
                {
                    NotificationChannel.Email => TimeSpan.FromHours(1),
                    NotificationChannel.SMS => TimeSpan.FromMinutes(30),
                    NotificationChannel.InApp => TimeSpan.FromMinutes(10),
                    _ => TimeSpan.FromHours(1)
                };

                return (delay <= expectedMaxDelay).Label($"Delay {delay} should not exceed max {expectedMaxDelay} for {channel}")
                    .And((delay > TimeSpan.Zero).Label("Delay is positive"))
                    .And((highAttemptNumber >= 10).Label("Testing with high attempt number"));
            });
    }

    #region Setup and Generators

    private void SetupMockConfiguration()
    {
        var configSection = new Mock<IConfigurationSection>();
        configSection.Setup(s => s.Exists()).Returns(false);
        _mockConfiguration.Setup(c => c.GetSection("NotificationRetryPolicies")).Returns(configSection.Object);
    }

    private static Arbitrary<(NotificationChannel channel, int maxAttempts)> GenerateRetryAttemptSequence()
    {
        return Arb.From(
            from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.SMS)
            from maxAttempts in Gen.Choose(2, 5) // Test with 2-5 attempts
            select (channel, maxAttempts));
    }

    private static Arbitrary<(NotificationChannel channel, int attemptNumber)> GenerateChannelAndAttemptNumber()
    {
        return Arb.From(
            from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.SMS)
            from attemptNumber in Gen.Choose(1, 10)
            select (channel, attemptNumber));
    }

    private static Arbitrary<(NotificationChannel channel, int attemptNumber, string errorMessage, bool isNonRetryable)> GenerateNonRetryableErrorScenario()
    {
        return Arb.From(
            from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.SMS)
            from attemptNumber in Gen.Choose(1, 3) // Within retry limits
            from isNonRetryable in Arb.Generate<bool>()
            from errorMessage in isNonRetryable 
                ? Gen.Elements("invalid email address", "user not found", "blocked recipient", "permanent failure", "invalid phone number")
                : Gen.Elements("network timeout", "temporary failure", "rate limited", "service unavailable")
            select (channel, attemptNumber, errorMessage, isNonRetryable));
    }

    private static Arbitrary<(NotificationChannel channel, int attemptNumber, int iterations)> GenerateJitterTestScenario()
    {
        return Arb.From(
            from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.SMS)
            from attemptNumber in Gen.Choose(1, 3)
            from iterations in Gen.Choose(10, 20) // Generate multiple delays to observe jitter
            select (channel, attemptNumber, iterations));
    }

    private static Arbitrary<(NotificationChannel channel, int highAttemptNumber)> GenerateHighAttemptNumberScenario()
    {
        return Arb.From(
            from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.SMS)
            from highAttemptNumber in Gen.Choose(10, 50) // Very high attempt numbers to test max delay cap
            select (channel, highAttemptNumber));
    }

    #endregion
}