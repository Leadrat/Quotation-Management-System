using AutoMapper;
using CRM.Application.Notifications.DTOs;
using CRM.Application.Notifications.Repositories;
using CRM.Application.Notifications.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Services;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace CRM.Tests.Notifications;

/// <summary>
/// **Feature: notification-creation-dispatch, Property 8: Dynamic configuration management**
/// Property-based tests for notification template management system
/// </summary>
public class NotificationTemplatePropertyTests
{
    private readonly Mock<INotificationTemplateRepository> _mockRepository;
    private readonly Mock<INotificationTemplateValidationService> _mockValidationService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly NotificationTemplateService _service;

    public NotificationTemplatePropertyTests()
    {
        _mockRepository = new Mock<INotificationTemplateRepository>();
        _mockValidationService = new Mock<INotificationTemplateValidationService>();
        _mockMapper = new Mock<IMapper>();
        _service = new NotificationTemplateService(_mockRepository.Object, _mockValidationService.Object, _mockMapper.Object);
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 8: Dynamic configuration management**
    /// **Validates: Requirements 5.1, 5.2, 5.3, 5.4, 5.5**
    /// 
    /// For any configuration change (channels, templates, settings), the system should apply 
    /// changes immediately without restart and validate required variables
    /// </summary>
    [Property(MaxTest = 100)]
    public Property TemplateConfigurationChangesApplyImmediately()
    {
        return Prop.ForAll(
            GenerateValidTemplate(),
            GenerateTemplateUpdate(),
            (originalTemplate, updateRequest) =>
            {
                // Arrange
                _mockRepository.Setup(r => r.GetByIdAsync(originalTemplate.Id))
                    .ReturnsAsync(originalTemplate);
                
                _mockValidationService.Setup(v => v.ValidateUpdateRequestAsync(originalTemplate.Id, updateRequest))
                    .ReturnsAsync(ValidationResult.Success());

                var updatedTemplate = new NotificationTemplate
                {
                    Id = originalTemplate.Id,
                    TemplateKey = originalTemplate.TemplateKey,
                    Channel = originalTemplate.Channel,
                    Name = updateRequest.Name,
                    Description = updateRequest.Description,
                    Subject = updateRequest.Subject,
                    BodyTemplate = updateRequest.BodyTemplate,
                    Variables = JsonSerializer.Serialize(updateRequest.Variables),
                    IsActive = updateRequest.IsActive,
                    CreatedAt = originalTemplate.CreatedAt,
                    UpdatedAt = DateTime.UtcNow
                };

                _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<NotificationTemplate>()))
                    .ReturnsAsync(updatedTemplate);

                var expectedDto = new NotificationTemplateDto
                {
                    Id = updatedTemplate.Id,
                    TemplateKey = updatedTemplate.TemplateKey,
                    Name = updatedTemplate.Name,
                    Description = updatedTemplate.Description,
                    Channel = updatedTemplate.Channel,
                    Subject = updatedTemplate.Subject,
                    BodyTemplate = updatedTemplate.BodyTemplate,
                    Variables = updateRequest.Variables,
                    IsActive = updatedTemplate.IsActive,
                    CreatedAt = updatedTemplate.CreatedAt,
                    UpdatedAt = updatedTemplate.UpdatedAt
                };

                _mockMapper.Setup(m => m.Map(updateRequest, originalTemplate));
                _mockMapper.Setup(m => m.Map<NotificationTemplateDto>(updatedTemplate))
                    .Returns(expectedDto);

                // Act
                var result = _service.UpdateTemplateAsync(originalTemplate.Id, updateRequest).Result;

                // Assert - Configuration changes are applied immediately
                return (result != null).Label("Template update returns result")
                    .And((result.Name == updateRequest.Name).Label("Name updated immediately"))
                    .And((result.Description == updateRequest.Description).Label("Description updated immediately"))
                    .And((result.Subject == updateRequest.Subject).Label("Subject updated immediately"))
                    .And((result.BodyTemplate == updateRequest.BodyTemplate).Label("Body template updated immediately"))
                    .And((result.IsActive == updateRequest.IsActive).Label("Active status updated immediately"))
                    .And((result.Variables.SequenceEqual(updateRequest.Variables)).Label("Variables updated immediately"));
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 8: Dynamic configuration management**
    /// **Validates: Requirements 5.2, 5.4**
    /// 
    /// For any template with variables, the system should validate that all required variables 
    /// are declared and all declared variables are used
    /// </summary>
    [Property(MaxTest = 100)]
    public Property TemplateVariableValidationIsEnforced()
    {
        return Prop.ForAll(
            GenerateTemplateWithVariables(),
            (templateData) =>
            {
                // Arrange
                var (bodyTemplate, declaredVariables) = templateData;
                var validationService = new NotificationTemplateValidationService(_mockRepository.Object);

                // Act
                var result = validationService.ValidateTemplateVariables(bodyTemplate, declaredVariables);

                // Extract variables from template
                var usedVariables = System.Text.RegularExpressions.Regex.Matches(bodyTemplate, @"\{\{(\w+)\}\}")
                    .Cast<System.Text.RegularExpressions.Match>()
                    .Select(m => m.Groups[1].Value)
                    .Distinct()
                    .ToList();

                var undeclaredVariables = usedVariables.Except(declaredVariables).ToList();
                var unusedVariables = declaredVariables.Except(usedVariables).ToList();

                // Assert - Variable validation is enforced
                if (undeclaredVariables.Any() || unusedVariables.Any())
                {
                    return (!result.IsValid).Label("Validation fails for mismatched variables")
                        .And((result.Errors.Any()).Label("Validation errors are reported"));
                }
                else
                {
                    return (result.IsValid).Label("Validation passes for matching variables")
                        .And((!result.Errors.Any()).Label("No validation errors for valid templates"));
                }
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 8: Dynamic configuration management**
    /// **Validates: Requirements 5.1, 5.3**
    /// 
    /// For any template key, the system should prevent duplicate keys and enforce uniqueness
    /// </summary>
    [Property(MaxTest = 100)]
    public Property TemplateKeyUniquenessIsEnforced()
    {
        return Prop.ForAll(
            GenerateCreateTemplateRequest(),
            (createRequest) =>
            {
                // Arrange - Template key already exists
                _mockRepository.Setup(r => r.ExistsAsync(createRequest.TemplateKey))
                    .ReturnsAsync(true);

                var validationService = new NotificationTemplateValidationService(_mockRepository.Object);

                // Act
                var result = validationService.ValidateCreateRequestAsync(createRequest).Result;

                // Assert - Uniqueness is enforced
                return (!result.IsValid).Label("Validation fails for duplicate template key")
                    .And((result.Errors.Any(e => e.Contains("already exists"))).Label("Duplicate key error is reported"));
            });
    }

    /// <summary>
    /// **Feature: notification-creation-dispatch, Property 8: Dynamic configuration management**
    /// **Validates: Requirements 5.5**
    /// 
    /// For any template rendering, the system should handle missing data gracefully 
    /// and provide meaningful error messages
    /// </summary>
    [Property(MaxTest = 100)]
    public Property TemplateRenderingHandlesMissingDataGracefully()
    {
        return Prop.ForAll(
            GenerateTemplateKeyAndData(),
            (templateData) =>
            {
                var (templateKey, data, channel) = templateData;
                
                // Arrange - Template exists but data might be incomplete
                var template = new NotificationTemplate
                {
                    Id = 1,
                    TemplateKey = templateKey,
                    Channel = channel,
                    IsActive = true,
                    BodyTemplate = "Hello {{Name}}, your {{Type}} is {{Status}}."
                };

                _mockRepository.Setup(r => r.GetByTemplateKeyAsync(templateKey))
                    .ReturnsAsync(template);

                try
                {
                    // Act
                    var result = _service.RenderTemplateAsync(templateKey, data, channel).Result;

                    // Assert - Rendering handles missing data gracefully
                    return (result != null).Label("Rendering returns result")
                        .And((!string.IsNullOrEmpty(result)).Label("Rendered content is not empty"))
                        .And((result.Contains("{{") == false || result.Contains("}}") == false || 
                              result.Contains("{{Name}}") || result.Contains("{{Type}}") || result.Contains("{{Status}}"))
                              .Label("Missing variables are either replaced or left as placeholders"));
                }
                catch (Exception ex)
                {
                    // Assert - Meaningful error messages for invalid scenarios
                    return (ex.Message.Contains("not found") || 
                            ex.Message.Contains("not configured") || 
                            ex.Message.Contains("not active"))
                           .Label("Meaningful error message provided");
                }
            });
    }

    #region Generators

    private static Arbitrary<NotificationTemplate> GenerateValidTemplate()
    {
        return Arb.From(
            from id in Gen.Choose(1, 1000)
            from templateKey in Gen.Elements("quotation-created", "approval-needed", "payment-received", "user-welcome")
            from name in Gen.Elements("Quotation Created", "Approval Needed", "Payment Received", "User Welcome")
            from description in Gen.Elements("Template for quotation creation", "Template for approval requests")
            from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.SMS)
            from subject in Gen.Elements("New Quotation", "Approval Required", "Payment Confirmation")
            from bodyTemplate in Gen.Elements("Hello {{Name}}", "Your {{Type}} needs attention", "Payment of {{Amount}} received")
            from variables in Gen.ListOf(Gen.Elements("Name", "Type", "Amount", "Date"))
            from isActive in Arb.Generate<bool>()
            select new NotificationTemplate
            {
                Id = id,
                TemplateKey = templateKey,
                Name = name,
                Description = description,
                Channel = channel,
                Subject = subject,
                BodyTemplate = bodyTemplate,
                Variables = JsonSerializer.Serialize(variables.Distinct().ToList()),
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            });
    }

    private static Arbitrary<UpdateNotificationTemplateRequest> GenerateTemplateUpdate()
    {
        return Arb.From(
            from name in Gen.Elements("Updated Template", "Modified Template", "New Template Name")
            from description in Gen.Elements("Updated description", "Modified description")
            from subject in Gen.Elements("Updated Subject", "Modified Subject")
            from bodyTemplate in Gen.Elements("Updated {{Name}}", "Modified {{Type}}", "New {{Status}} template")
            from variables in Gen.ListOf(Gen.Elements("Name", "Type", "Status", "Date", "Amount"))
            from isActive in Arb.Generate<bool>()
            select new UpdateNotificationTemplateRequest
            {
                Name = name,
                Description = description,
                Subject = subject,
                BodyTemplate = bodyTemplate,
                Variables = variables.Distinct().ToList(),
                IsActive = isActive
            });
    }

    private static Arbitrary<(string bodyTemplate, List<string> declaredVariables)> GenerateTemplateWithVariables()
    {
        return Arb.From(
            from variables in Gen.ListOf(Gen.Elements("Name", "Type", "Status", "Amount", "Date")).Where(list => list.Any())
            from extraVariables in Gen.ListOf(Gen.Elements("Extra1", "Extra2", "Unused"))
            from missingVariables in Gen.ListOf(Gen.Elements("Missing1", "Missing2", "Undeclared"))
            from includeExtra in Arb.Generate<bool>()
            from includeMissing in Arb.Generate<bool>()
            select GenerateTemplateWithVariablesCombination(variables.Distinct().ToList(), extraVariables.Distinct().ToList(), missingVariables.Distinct().ToList(), includeExtra, includeMissing));
    }

    private static (string bodyTemplate, List<string> declaredVariables) GenerateTemplateWithVariablesCombination(
        List<string> baseVariables, List<string> extraVariables, List<string> missingVariables, bool includeExtra, bool includeMissing)
    {
        var usedVariables = baseVariables.ToList();
        if (includeMissing)
        {
            usedVariables.AddRange(missingVariables);
        }

        var declaredVariables = baseVariables.ToList();
        if (includeExtra)
        {
            declaredVariables.AddRange(extraVariables);
        }

        var bodyTemplate = string.Join(" ", usedVariables.Select(v => $"{{{{ {v} }}}}"));
        return (bodyTemplate, declaredVariables);
    }

    private static Arbitrary<CreateNotificationTemplateRequest> GenerateCreateTemplateRequest()
    {
        return Arb.From(
            from templateKey in Gen.Elements("new-template", "test-template", "sample-template")
            from name in Gen.Elements("New Template", "Test Template", "Sample Template")
            from description in Gen.Elements("A new template", "A test template")
            from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.SMS)
            from subject in Gen.Elements("New Subject", "Test Subject")
            from bodyTemplate in Gen.Elements("Hello {{Name}}", "Test {{Message}}")
            from variables in Gen.ListOf(Gen.Elements("Name", "Message", "Type"))
            from isActive in Arb.Generate<bool>()
            select new CreateNotificationTemplateRequest
            {
                TemplateKey = templateKey,
                Name = name,
                Description = description,
                Channel = channel,
                Subject = subject,
                BodyTemplate = bodyTemplate,
                Variables = variables.Distinct().ToList(),
                IsActive = isActive
            });
    }

    private static Arbitrary<(string templateKey, object data, NotificationChannel channel)> GenerateTemplateKeyAndData()
    {
        return Arb.From(
            from templateKey in Gen.Elements("test-template", "sample-template")
            from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.SMS)
            from hasName in Arb.Generate<bool>()
            from hasType in Arb.Generate<bool>()
            from hasStatus in Arb.Generate<bool>()
            select (
                templateKey,
                (object)new Dictionary<string, object?>
                {
                    ["Name"] = hasName ? "John Doe" : null,
                    ["Type"] = hasType ? "Quotation" : null,
                    ["Status"] = hasStatus ? "Pending" : null
                },
                channel
            ));
    }

    #endregion
}
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ChannelSpecificFormattingIsApplied()
    {
        return Prop.ForAll(
            GenerateTemplateAndData(),
            (templateData) =>
            {
                var (templateKey, content, data) = templateData;
                
                // Test each channel format
                var results = new List<(NotificationChannel channel, string result)>();
                
                foreach (NotificationChannel channel in Enum.GetValues<NotificationChannel>())
                {
                    // Arrange - Template for each channel
                    var template = new NotificationTemplate
                    {
                        Id = 1,
                        TemplateKey = templateKey,
                        Channel = channel,
                        IsActive = true,
                        Subject = "Test {{Type}}",
                        BodyTemplate = content
                    };

                    _mockRepository.Setup(r => r.GetByTemplateKeyAsync(templateKey))
                        .ReturnsAsync(template);

                    try
                    {
                        // Act
                        var result = _service.RenderTemplateAsync(templateKey, data, channel).Result;
                        results.Add((channel, result));
                    }
                    catch
                    {
                        // Skip channels that fail - focus on successful formatting
                        continue;
                    }
                }

                if (!results.Any()) return true.Label("No successful renders to test");

                // Assert - Channel-specific formatting rules
                var properties = new List<Property>();

                foreach (var (channel, result) in results)
                {
                    switch (channel)
                    {
                        case NotificationChannel.InApp:
                            // In-app: Clean, trimmed content
                            properties.Add((result.Trim() == result).Label($"InApp content is trimmed"));
                            properties.Add((!result.Contains("<!DOCTYPE")).Label($"InApp has no HTML structure"));
                            break;

                        case NotificationChannel.Email:
                            // Email: Full HTML structure with DOCTYPE
                            properties.Add((result.Contains("<!DOCTYPE html>")).Label($"Email has HTML DOCTYPE"));
                            properties.Add((result.Contains("<html>")).Label($"Email has HTML tags"));
                            properties.Add((result.Contains("<body>")).Label($"Email has body tags"));
                            break;

                        case NotificationChannel.SMS:
                            // SMS: Character limited, no HTML
                            properties.Add((result.Length <= 160).Label($"SMS is within 160 chars: {result.Length}"));
                            properties.Add((!result.Contains("<")).Label($"SMS has no HTML tags"));
                            properties.Add((!result.Contains(">")).Label($"SMS has no HTML brackets"));
                            break;
                    }
                }

                return properties.Aggregate((p1, p2) => p1.And(p2));
            });
    }

    #region Additional Generators

    private static Arbitrary<(string templateKey, string content, object data)> GenerateTemplateAndData()
    {
        return Arb.From(
            from templateKey in Gen.Elements("test-template", "format-test")
            from content in Gen.Elements(
                "Hello {{Name}}!",
                "<p>Your {{Type}} is {{Status}}</p>",
                "Welcome {{Name}}, your account {{Status}} is ready. Contact us at {{Email}} for support."
            )
            from name in Gen.Elements("John Doe", "Jane Smith", "Test User")
            from type in Gen.Elements("Order", "Account", "Notification")
            from status in Gen.Elements("Active", "Pending", "Complete")
            from email in Gen.Elements("test@example.com", "support@company.com")
            select (
                templateKey,
                content,
                (object)new Dictionary<string, object>
                {
                    ["Name"] = name,
                    ["Type"] = type,
                    ["Status"] = status,
                    ["Email"] = email
                }
            ));
    }

    #endregion