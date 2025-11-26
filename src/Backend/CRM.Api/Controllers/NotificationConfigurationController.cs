using CRM.Application.Notifications.DTOs;
using CRM.Application.Notifications.Services;
using CRM.Application.Notifications.Repositories;
using CRM.Domain.Enums;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for notification configuration management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationConfigurationController : ControllerBase
{
    private readonly INotificationTemplateService _templateService;
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly INotificationChannelConfigurationService _channelConfigService;
    private readonly ILogger<NotificationConfigurationController> _logger;

    public NotificationConfigurationController(
        INotificationTemplateService templateService,
        INotificationTemplateRepository templateRepository,
        INotificationChannelConfigurationService channelConfigService,
        ILogger<NotificationConfigurationController> logger)
    {
        _templateService = templateService;
        _templateRepository = templateRepository;
        _channelConfigService = channelConfigService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all notification templates
    /// </summary>
    [HttpGet("templates")]
    public async Task<ActionResult<List<NotificationTemplateDto>>> GetTemplates(
        [FromQuery] NotificationChannel? channel = null,
        [FromQuery] bool? isActive = null)
    {
        try
        {
            // This would need to be implemented in the template service
            // For now, return a simple response
            var templates = new List<NotificationTemplateDto>();
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification templates");
            return StatusCode(500, "An error occurred while retrieving templates");
        }
    }

    /// <summary>
    /// Gets a specific notification template
    /// </summary>
    [HttpGet("templates/{templateKey}")]
    public async Task<ActionResult<NotificationTemplateDto>> GetTemplate(string templateKey)
    {
        try
        {
            // This would need to be implemented in the template service
            return NotFound($"Template '{templateKey}' not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification template {TemplateKey}", templateKey);
            return StatusCode(500, "An error occurred while retrieving the template");
        }
    }

    /// <summary>
    /// Creates a new notification template
    /// </summary>
    [HttpPost("templates")]
    public async Task<ActionResult<NotificationTemplateDto>> CreateTemplate(
        [FromBody] CreateNotificationTemplateRequest request)
    {
        try
        {
            // Validate the request
            if (string.IsNullOrEmpty(request.TemplateKey))
            {
                return BadRequest("Template key is required");
            }

            if (string.IsNullOrEmpty(request.BodyTemplate))
            {
                return BadRequest("Body template is required");
            }

            // This would need to be implemented in the template service
            return StatusCode(501, "Template creation not yet implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification template");
            return StatusCode(500, "An error occurred while creating the template");
        }
    }

    /// <summary>
    /// Updates an existing notification template
    /// </summary>
    [HttpPut("templates/{templateKey}")]
    public async Task<ActionResult<NotificationTemplateDto>> UpdateTemplate(
        string templateKey,
        [FromBody] UpdateNotificationTemplateRequest request)
    {
        try
        {
            // This would need to be implemented in the template service
            return StatusCode(501, "Template update not yet implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification template {TemplateKey}", templateKey);
            return StatusCode(500, "An error occurred while updating the template");
        }
    }

    /// <summary>
    /// Deletes a notification template
    /// </summary>
    [HttpDelete("templates/{templateKey}")]
    public async Task<ActionResult> DeleteTemplate(string templateKey)
    {
        try
        {
            // This would need to be implemented in the template service
            return StatusCode(501, "Template deletion not yet implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification template {TemplateKey}", templateKey);
            return StatusCode(500, "An error occurred while deleting the template");
        }
    }

    /// <summary>
    /// Tests a notification template with sample data
    /// </summary>
    [HttpPost("templates/{templateKey}/test")]
    public async Task<ActionResult<string>> TestTemplate(
        string templateKey,
        [FromBody] TestTemplateRequest request)
    {
        try
        {
            var renderedContent = await _templateService.RenderTemplateAsync(
                templateKey,
                request.TestData,
                request.Channel);

            return Ok(new { RenderedContent = renderedContent });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing notification template {TemplateKey}", templateKey);
            return StatusCode(500, "An error occurred while testing the template");
        }
    }

    /// <summary>
    /// Gets available template variables for a specific template type
    /// </summary>
    [HttpGet("templates/variables/{eventType}")]
    public async Task<ActionResult<List<string>>> GetTemplateVariables(string eventType)
    {
        try
        {
            // This would return available variables based on the event type
            var variables = eventType.ToLowerInvariant() switch
            {
                "quotation-created" => new List<string> { "QuotationId", "UserId", "ClientId", "Amount", "QuotationNumber", "CreatedAt" },
                "quotation-sent" => new List<string> { "QuotationId", "UserId", "ClientId", "ClientEmail", "QuotationNumber", "SentAt" },
                "approval-needed" => new List<string> { "EntityId", "EntityType", "RequesterId", "ApproverId", "Reason", "RequestedAt" },
                "payment-received" => new List<string> { "PaymentId", "UserId", "QuotationId", "Amount", "PaymentMethod", "ReceivedAt" },
                "user-account-created" => new List<string> { "UserId", "Email", "FirstName", "LastName", "CreatedAt" },
                _ => new List<string>()
            };

            return Ok(variables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template variables for event type {EventType}", eventType);
            return StatusCode(500, "An error occurred while retrieving template variables");
        }
    }

    /// <summary>
    /// Gets all channel configurations
    /// </summary>
    [HttpGet("channels")]
    public async Task<ActionResult<List<ChannelConfiguration>>> GetChannelConfigurations()
    {
        try
        {
            var configurations = await _channelConfigService.GetAllChannelConfigurationsAsync();
            return Ok(configurations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving channel configurations");
            return StatusCode(500, "An error occurred while retrieving channel configurations");
        }
    }

    /// <summary>
    /// Gets configuration for a specific channel
    /// </summary>
    [HttpGet("channels/{channel}")]
    public async Task<ActionResult<ChannelConfiguration>> GetChannelConfiguration(NotificationChannel channel)
    {
        try
        {
            var configuration = await _channelConfigService.GetChannelConfigurationAsync(channel);
            return Ok(configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration for channel {Channel}", channel);
            return StatusCode(500, "An error occurred while retrieving channel configuration");
        }
    }

    /// <summary>
    /// Updates channel configuration
    /// </summary>
    [HttpPut("channels/{channel}")]
    public async Task<ActionResult<ChannelConfiguration>> UpdateChannelConfiguration(
        NotificationChannel channel,
        [FromBody] ChannelConfiguration configuration)
    {
        try
        {
            if (configuration.Channel != channel)
            {
                return BadRequest("Channel in URL must match channel in configuration");
            }

            var updatedConfiguration = await _channelConfigService.UpdateChannelConfigurationAsync(channel, configuration);
            return Ok(updatedConfiguration);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid configuration for channel {Channel}", channel);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration for channel {Channel}", channel);
            return StatusCode(500, "An error occurred while updating channel configuration");
        }
    }

    /// <summary>
    /// Tests a channel configuration
    /// </summary>
    [HttpPost("channels/{channel}/test")]
    public async Task<ActionResult<ConfigurationTestResult>> TestChannelConfiguration(
        NotificationChannel channel,
        [FromBody] ChannelConfiguration configuration)
    {
        try
        {
            var testResult = await _channelConfigService.TestChannelConfigurationAsync(channel, configuration);
            return Ok(testResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing configuration for channel {Channel}", channel);
            return StatusCode(500, "An error occurred while testing channel configuration");
        }
    }

    /// <summary>
    /// Reloads all channel configurations (hot reload)
    /// </summary>
    [HttpPost("reload")]
    [Authorize(Roles = "Administrator,SystemAdministrator")]
    public async Task<ActionResult> ReloadConfigurations()
    {
        try
        {
            await _channelConfigService.ReloadConfigurationsAsync();
            return Ok(new { Message = "Configurations reloaded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading configurations");
            return StatusCode(500, "An error occurred while reloading configurations");
        }
    }
}

/// <summary>
/// Request model for creating notification templates
/// </summary>
public class CreateNotificationTemplateRequest
{
    public string TemplateKey { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request model for updating notification templates
/// </summary>
public class UpdateNotificationTemplateRequest
{
    public string? Subject { get; set; }
    public string? BodyTemplate { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Request model for testing notification templates
/// </summary>
public class TestTemplateRequest
{
    public NotificationChannel Channel { get; set; }
    public Dictionary<string, object> TestData { get; set; } = new();
}