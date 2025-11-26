using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing notification channel configurations with hot reloading
/// </summary>
public class NotificationChannelConfigurationService : INotificationChannelConfigurationService
{
    private readonly IAppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationChannelConfigurationService> _logger;
    private readonly Dictionary<NotificationChannel, ChannelConfiguration> _cachedConfigurations;
    private readonly object _cacheLock = new();
    private DateTime _lastCacheUpdate = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    public NotificationChannelConfigurationService(
        IAppDbContext context,
        IConfiguration configuration,
        ILogger<NotificationChannelConfigurationService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _cachedConfigurations = new Dictionary<NotificationChannel, ChannelConfiguration>();
    }

    public async Task<ChannelConfiguration> GetChannelConfigurationAsync(
        NotificationChannel channel,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check cache first
            lock (_cacheLock)
            {
                if (_cachedConfigurations.TryGetValue(channel, out var cachedConfig) &&
                    DateTime.UtcNow - _lastCacheUpdate < _cacheExpiry)
                {
                    return cachedConfig;
                }
            }

            // Load from database
            var dbConfig = await _context.NotificationChannelConfigurations
                .FirstOrDefaultAsync(c => c.Channel == channel && c.IsEnabled, cancellationToken);

            ChannelConfiguration configuration;
            
            if (dbConfig != null)
            {
                configuration = JsonSerializer.Deserialize<ChannelConfiguration>(dbConfig.Configuration) 
                    ?? GetDefaultConfiguration(channel);
            }
            else
            {
                // Fall back to default configuration
                configuration = GetDefaultConfiguration(channel);
            }

            // Update cache
            lock (_cacheLock)
            {
                _cachedConfigurations[channel] = configuration;
                _lastCacheUpdate = DateTime.UtcNow;
            }

            _logger.LogDebug("Loaded configuration for channel {Channel}", channel);
            return configuration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration for channel {Channel}", channel);
            return GetDefaultConfiguration(channel);
        }
    }

    public async Task<ChannelConfiguration> UpdateChannelConfigurationAsync(
        NotificationChannel channel,
        ChannelConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating configuration for channel {Channel}", channel);

            // Validate configuration
            var validationResult = ValidateConfiguration(channel, configuration);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"Invalid configuration: {string.Join(", ", validationResult.Errors)}");
            }

            var configJson = JsonSerializer.Serialize(configuration);
            
            // Update or create database record
            var existingConfig = await _context.NotificationChannelConfigurations
                .FirstOrDefaultAsync(c => c.Channel == channel, cancellationToken);

            if (existingConfig != null)
            {
                existingConfig.Configuration = configJson;
                existingConfig.UpdatedAt = DateTimeOffset.UtcNow;
                existingConfig.IsEnabled = true;
            }
            else
            {
                var newConfig = new NotificationChannelConfiguration
                {
                    Channel = channel,
                    Configuration = configJson,
                    IsEnabled = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                _context.NotificationChannelConfigurations.Add(newConfig);
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Update cache immediately (hot reload)
            lock (_cacheLock)
            {
                _cachedConfigurations[channel] = configuration;
                _lastCacheUpdate = DateTime.UtcNow;
            }

            _logger.LogInformation("Successfully updated configuration for channel {Channel}", channel);
            return configuration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration for channel {Channel}", channel);
            throw;
        }
    }

    public async Task<List<ChannelConfiguration>> GetAllChannelConfigurationsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var configurations = new List<ChannelConfiguration>();
            
            foreach (var channel in Enum.GetValues<NotificationChannel>())
            {
                var config = await GetChannelConfigurationAsync(channel, cancellationToken);
                config.Channel = channel;
                configurations.Add(config);
            }

            return configurations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading all channel configurations");
            throw;
        }
    }

    public async Task<ConfigurationTestResult> TestChannelConfigurationAsync(
        NotificationChannel channel,
        ChannelConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Testing configuration for channel {Channel}", channel);

            var result = new ConfigurationTestResult
            {
                Channel = channel,
                IsValid = true,
                TestResults = new List<string>()
            };

            // Validate configuration structure
            var validationResult = ValidateConfiguration(channel, configuration);
            if (!validationResult.IsValid)
            {
                result.IsValid = false;
                result.TestResults.AddRange(validationResult.Errors);
                return result;
            }

            // Perform channel-specific tests
            switch (channel)
            {
                case NotificationChannel.Email:
                    await TestEmailConfiguration(configuration, result, cancellationToken);
                    break;
                case NotificationChannel.SMS:
                    await TestSmsConfiguration(configuration, result, cancellationToken);
                    break;
                case NotificationChannel.InApp:
                    await TestInAppConfiguration(configuration, result, cancellationToken);
                    break;
            }

            _logger.LogInformation("Configuration test for channel {Channel} completed: {IsValid}", 
                channel, result.IsValid);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing configuration for channel {Channel}", channel);
            
            return new ConfigurationTestResult
            {
                Channel = channel,
                IsValid = false,
                TestResults = new List<string> { $"Test failed with error: {ex.Message}" }
            };
        }
    }

    public async Task ReloadConfigurationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Reloading all channel configurations");

            // Clear cache to force reload
            lock (_cacheLock)
            {
                _cachedConfigurations.Clear();
                _lastCacheUpdate = DateTime.MinValue;
            }

            // Pre-load all configurations
            foreach (var channel in Enum.GetValues<NotificationChannel>())
            {
                await GetChannelConfigurationAsync(channel, cancellationToken);
            }

            _logger.LogInformation("Successfully reloaded all channel configurations");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading channel configurations");
            throw;
        }
    }

    private static ChannelConfiguration GetDefaultConfiguration(NotificationChannel channel)
    {
        return channel switch
        {
            NotificationChannel.Email => new ChannelConfiguration
            {
                Channel = channel,
                IsEnabled = true,
                MaxRetryAttempts = 3,
                RetryDelaySeconds = 60,
                TimeoutSeconds = 30,
                Settings = new Dictionary<string, object>
                {
                    ["SmtpServer"] = "localhost",
                    ["SmtpPort"] = 587,
                    ["UseSsl"] = true,
                    ["FromAddress"] = "noreply@example.com",
                    ["FromName"] = "CRM System"
                }
            },
            NotificationChannel.SMS => new ChannelConfiguration
            {
                Channel = channel,
                IsEnabled = true,
                MaxRetryAttempts = 2,
                RetryDelaySeconds = 120,
                TimeoutSeconds = 15,
                Settings = new Dictionary<string, object>
                {
                    ["Provider"] = "Twilio",
                    ["MaxLength"] = 160,
                    ["FromNumber"] = "+1234567890"
                }
            },
            NotificationChannel.InApp => new ChannelConfiguration
            {
                Channel = channel,
                IsEnabled = true,
                MaxRetryAttempts = 1,
                RetryDelaySeconds = 5,
                TimeoutSeconds = 10,
                Settings = new Dictionary<string, object>
                {
                    ["ConnectionTimeout"] = 30,
                    ["MaxConnections"] = 1000
                }
            },
            _ => new ChannelConfiguration
            {
                Channel = channel,
                IsEnabled = false,
                MaxRetryAttempts = 1,
                RetryDelaySeconds = 60,
                TimeoutSeconds = 30,
                Settings = new Dictionary<string, object>()
            }
        };
    }

    private static ConfigurationValidationResult ValidateConfiguration(
        NotificationChannel channel, 
        ChannelConfiguration configuration)
    {
        var result = new ConfigurationValidationResult { IsValid = true, Errors = new List<string>() };

        if (configuration == null)
        {
            result.IsValid = false;
            result.Errors.Add("Configuration cannot be null");
            return result;
        }

        if (configuration.MaxRetryAttempts < 0 || configuration.MaxRetryAttempts > 10)
        {
            result.IsValid = false;
            result.Errors.Add("MaxRetryAttempts must be between 0 and 10");
        }

        if (configuration.RetryDelaySeconds < 1 || configuration.RetryDelaySeconds > 3600)
        {
            result.IsValid = false;
            result.Errors.Add("RetryDelaySeconds must be between 1 and 3600");
        }

        if (configuration.TimeoutSeconds < 1 || configuration.TimeoutSeconds > 300)
        {
            result.IsValid = false;
            result.Errors.Add("TimeoutSeconds must be between 1 and 300");
        }

        // Channel-specific validation
        switch (channel)
        {
            case NotificationChannel.Email:
                ValidateEmailSettings(configuration.Settings, result);
                break;
            case NotificationChannel.SMS:
                ValidateSmsSettings(configuration.Settings, result);
                break;
        }

        return result;
    }

    private static void ValidateEmailSettings(Dictionary<string, object> settings, ConfigurationValidationResult result)
    {
        if (!settings.ContainsKey("SmtpServer") || string.IsNullOrEmpty(settings["SmtpServer"]?.ToString()))
        {
            result.IsValid = false;
            result.Errors.Add("SmtpServer is required for email configuration");
        }

        if (!settings.ContainsKey("SmtpPort") || !int.TryParse(settings["SmtpPort"]?.ToString(), out var port) || port <= 0)
        {
            result.IsValid = false;
            result.Errors.Add("Valid SmtpPort is required for email configuration");
        }
    }

    private static void ValidateSmsSettings(Dictionary<string, object> settings, ConfigurationValidationResult result)
    {
        if (!settings.ContainsKey("Provider") || string.IsNullOrEmpty(settings["Provider"]?.ToString()))
        {
            result.IsValid = false;
            result.Errors.Add("Provider is required for SMS configuration");
        }

        if (!settings.ContainsKey("MaxLength") || !int.TryParse(settings["MaxLength"]?.ToString(), out var maxLength) || maxLength <= 0)
        {
            result.IsValid = false;
            result.Errors.Add("Valid MaxLength is required for SMS configuration");
        }
    }

    private async Task TestEmailConfiguration(ChannelConfiguration configuration, ConfigurationTestResult result, CancellationToken cancellationToken)
    {
        // Test email configuration by attempting to connect to SMTP server
        try
        {
            var smtpServer = configuration.Settings["SmtpServer"]?.ToString();
            var smtpPort = int.Parse(configuration.Settings["SmtpPort"]?.ToString() ?? "587");
            
            result.TestResults.Add($"SMTP Server: {smtpServer}:{smtpPort} - Connection test would be performed here");
            // In a real implementation, you would test the SMTP connection
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.TestResults.Add($"Email configuration test failed: {ex.Message}");
        }
    }

    private async Task TestSmsConfiguration(ChannelConfiguration configuration, ConfigurationTestResult result, CancellationToken cancellationToken)
    {
        // Test SMS configuration by validating provider settings
        try
        {
            var provider = configuration.Settings["Provider"]?.ToString();
            result.TestResults.Add($"SMS Provider: {provider} - Configuration validation passed");
            // In a real implementation, you would test the SMS provider API
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.TestResults.Add($"SMS configuration test failed: {ex.Message}");
        }
    }

    private async Task TestInAppConfiguration(ChannelConfiguration configuration, ConfigurationTestResult result, CancellationToken cancellationToken)
    {
        // Test in-app configuration
        try
        {
            result.TestResults.Add("In-app notification configuration validated successfully");
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.TestResults.Add($"In-app configuration test failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Interface for notification channel configuration management
/// </summary>
public interface INotificationChannelConfigurationService
{
    Task<ChannelConfiguration> GetChannelConfigurationAsync(NotificationChannel channel, CancellationToken cancellationToken = default);
    Task<ChannelConfiguration> UpdateChannelConfigurationAsync(NotificationChannel channel, ChannelConfiguration configuration, CancellationToken cancellationToken = default);
    Task<List<ChannelConfiguration>> GetAllChannelConfigurationsAsync(CancellationToken cancellationToken = default);
    Task<ConfigurationTestResult> TestChannelConfigurationAsync(NotificationChannel channel, ChannelConfiguration configuration, CancellationToken cancellationToken = default);
    Task ReloadConfigurationsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Configuration for a notification channel
/// </summary>
public class ChannelConfiguration
{
    public NotificationChannel Channel { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 60;
    public int TimeoutSeconds { get; set; } = 30;
    public Dictionary<string, object> Settings { get; set; } = new();
}

/// <summary>
/// Result of configuration validation
/// </summary>
public class ConfigurationValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Result of configuration testing
/// </summary>
public class ConfigurationTestResult
{
    public NotificationChannel Channel { get; set; }
    public bool IsValid { get; set; }
    public List<string> TestResults { get; set; } = new();
}