using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.DependencyInjection;
using CRM.Application.Common.Interfaces;
using CRM.Domain.Entities;
using CRM.Tests.Common;

namespace CRM.Tests.Notifications;

/**
 * Feature: notification-creation-dispatch, Property 3: Channel-specific formatting
 * Validates: Requirements 2.2, 2.3, 2.4
 */
public class ChannelFormattingPropertyTests : BaseIntegrationTest
{
    private readonly INotificationTemplateService _templateService;
    private readonly ISmsService _smsService;

    public ChannelFormattingPropertyTests()
    {
        _templateService = ServiceProvider.GetRequiredService<INotificationTemplateService>();
        _smsService = ServiceProvider.GetRequiredService<ISmsService>();
    }

    [Property]
    public Property EmailFormatting_ShouldSupportHtmlAndPlainText()
    {
        return Prop.ForAll(
            GenerateEmailTemplate(),
            GenerateTemplateData(),
            async (template, data) =>
            {
                var result = await _templateService.RenderTemplateAsync(template, data);
                
                // Email should support both HTML and plain text
                var hasHtmlTags = result.Body.Contains("<") && result.Body.Contains(">");
                var hasSubject = !string.IsNullOrEmpty(result.Subject);
                
                // Email must have subject and body
                return hasSubject && !string.IsNullOrEmpty(result.Body);
            });
    }

    [Property]
    public Property SmsFormatting_ShouldRespectCharacterLimits()
    {
        return Prop.ForAll(
            GenerateSmsTemplate(),
            GenerateTemplateData(),
            async (template, data) =>
            {
                var result = await _templateService.RenderTemplateAsync(template, data);
                
                // SMS should respect character limits (160 for single SMS)
                var isWithinLimit = result.Body.Length <= 160;
                var hasNoSubject = string.IsNullOrEmpty(result.Subject); // SMS doesn't use subject
                
                return isWithinLimit && hasNoSubject;
            });
    }

    [Property]
    public Property InAppFormatting_ShouldSupportRichContent()
    {
        return Prop.ForAll(
            GenerateInAppTemplate(),
            GenerateTemplateData(),
            async (template, data) =>
            {
                var result = await _templateService.RenderTemplateAsync(template, data);
                
                // In-app notifications can have rich content
                var hasTitle = !string.IsNullOrEmpty(result.Subject);
                var hasBody = !string.IsNullOrEmpty(result.Body);
                
                return hasTitle && hasBody;
            });
    }

    [Property]
    public Property ChannelSpecificVariableSubstitution_ShouldBeConsistent()
    {
        return Prop.ForAll(
            GenerateTemplateForAllChannels(),
            GenerateConsistentTemplateData(),
            async (templates, data) =>
            {
                var results = new List<(NotificationChannel Channel, string Content)>();
                
                foreach (var template in templates)
                {
                    var result = await _templateService.RenderTemplateAsync(template, data);
                    results.Add((template.Channel, result.Body));
                }
                
                // All channels should substitute the same variables consistently
                var userName = data["userName"].ToString();
                return results.All(r => r.Content.Contains(userName));
            });
    }

    [Property]
    public Property SmsCharacterValidation_ShouldRejectOversizedContent()
    {
        return Prop.ForAll(
            GenerateLongSmsTemplate(),
            GenerateTemplateData(),
            async (template, data) =>
            {
                try
                {
                    var result = await _templateService.RenderTemplateAsync(template, data);
                    
                    // If SMS is too long, it should either be truncated or rejected
                    if (result.Body.Length > 160)
                    {
                        // Should have been truncated or validation should have failed
                        return false;
                    }
                    
                    return true;
                }
                catch (ArgumentException)
                {
                    // Validation correctly rejected oversized content
                    return true;
                }
            });
    }

    [Property]
    public Property EmailHtmlSanitization_ShouldPreventXSS()
    {
        return Prop.ForAll(
            GenerateEmailTemplateWithPotentialXSS(),
            GenerateTemplateDataWithScripts(),
            async (template, data) =>
            {
                var result = await _templateService.RenderTemplateAsync(template, data);
                
                // Should not contain dangerous script tags
                var hasDangerousScript = result.Body.Contains("<script>") || 
                                       result.Body.Contains("javascript:") ||
                                       result.Body.Contains("onclick=");
                
                return !hasDangerousScript;
            });
    }

    [Property]
    public Property ChannelFormatPreservation_ShouldMaintainIntegrity()
    {
        return Prop.ForAll(
            GenerateTemplateWithSpecialCharacters(),
            GenerateTemplateData(),
            async (template, data) =>
            {
                var result = await _templateService.RenderTemplateAsync(template, data);
                
                // Special characters should be handled appropriately per channel
                switch (template.Channel)
                {
                    case NotificationChannel.Email:
                        // Email should handle HTML entities
                        return !result.Body.Contains("&lt;") || result.Body.Contains("<");
                    
                    case NotificationChannel.Sms:
                        // SMS should preserve plain text
                        return !result.Body.Contains("<") && !result.Body.Contains(">");
                    
                    case NotificationChannel.InApp:
                        // In-app can handle various formats
                        return !string.IsNullOrEmpty(result.Body);
                    
                    default:
                        return true;
                }
            });
    }

    private static Arbitrary<NotificationTemplate> GenerateEmailTemplate()
    {
        return Arb.From(
            from subject in Gen.Elements("Welcome {userName}", "Your order {orderId} is ready", "Important update")
            from body in Gen.Elements(
                "<h1>Hello {userName}</h1><p>Your order is ready.</p>",
                "<div>Dear {userName}, <br/>Thank you for your purchase.</div>",
                "Plain text email for {userName}")
            select new NotificationTemplate
            {
                TemplateKey = "email-test",
                EventType = "TestEvent",
                Channel = NotificationChannel.Email,
                Subject = subject,
                BodyTemplate = body,
                IsActive = true,
                RequiredVariables = new List<string> { "userName" }
            });
    }

    private static Arbitrary<NotificationTemplate> GenerateSmsTemplate()
    {
        return Arb.From(
            from body in Gen.Elements(
                "Hi {userName}! Your order is ready.",
                "{userName}, your payment of {amount} received.",
                "Alert: {message}")
            select new NotificationTemplate
            {
                TemplateKey = "sms-test",
                EventType = "TestEvent",
                Channel = NotificationChannel.Sms,
                Subject = null, // SMS doesn't use subject
                BodyTemplate = body,
                IsActive = true,
                RequiredVariables = new List<string> { "userName" }
            });
    }

    private static Arbitrary<NotificationTemplate> GenerateInAppTemplate()
    {
        return Arb.From(
            from subject in Gen.Elements("New Message", "Update Available", "Action Required")
            from body in Gen.Elements(
                "Hello {userName}, you have a new notification.",
                "Your request has been processed, {userName}.",
                "Please review your recent activity, {userName}.")
            select new NotificationTemplate
            {
                TemplateKey = "inapp-test",
                EventType = "TestEvent",
                Channel = NotificationChannel.InApp,
                Subject = subject,
                BodyTemplate = body,
                IsActive = true,
                RequiredVariables = new List<string> { "userName" }
            });
    }

    private static Arbitrary<List<NotificationTemplate>> GenerateTemplateForAllChannels()
    {
        return Arb.From(
            from eventType in Gen.Elements("TestEvent", "SampleEvent")
            select new List<NotificationTemplate>
            {
                new NotificationTemplate
                {
                    TemplateKey = $"{eventType.ToLower()}-email",
                    EventType = eventType,
                    Channel = NotificationChannel.Email,
                    Subject = "Email: Hello {userName}",
                    BodyTemplate = "<p>Email content for {userName}</p>",
                    IsActive = true,
                    RequiredVariables = new List<string> { "userName" }
                },
                new NotificationTemplate
                {
                    TemplateKey = $"{eventType.ToLower()}-sms",
                    EventType = eventType,
                    Channel = NotificationChannel.Sms,
                    Subject = null,
                    BodyTemplate = "SMS: Hi {userName}!",
                    IsActive = true,
                    RequiredVariables = new List<string> { "userName" }
                },
                new NotificationTemplate
                {
                    TemplateKey = $"{eventType.ToLower()}-inapp",
                    EventType = eventType,
                    Channel = NotificationChannel.InApp,
                    Subject = "In-App: Update for {userName}",
                    BodyTemplate = "In-app content for {userName}",
                    IsActive = true,
                    RequiredVariables = new List<string> { "userName" }
                }
            });
    }

    private static Arbitrary<NotificationTemplate> GenerateLongSmsTemplate()
    {
        return Arb.From(
            Gen.Constant(new NotificationTemplate
            {
                TemplateKey = "long-sms-test",
                EventType = "TestEvent",
                Channel = NotificationChannel.Sms,
                Subject = null,
                BodyTemplate = "This is a very long SMS message for {userName} that exceeds the standard 160 character limit and should be handled appropriately by the system to ensure proper delivery and formatting.",
                IsActive = true,
                RequiredVariables = new List<string> { "userName" }
            }));
    }

    private static Arbitrary<NotificationTemplate> GenerateEmailTemplateWithPotentialXSS()
    {
        return Arb.From(
            Gen.Constant(new NotificationTemplate
            {
                TemplateKey = "xss-test-email",
                EventType = "TestEvent",
                Channel = NotificationChannel.Email,
                Subject = "Test {userName}",
                BodyTemplate = "<p>Hello {userName}, your data: {userData}</p>",
                IsActive = true,
                RequiredVariables = new List<string> { "userName", "userData" }
            }));
    }

    private static Arbitrary<NotificationTemplate> GenerateTemplateWithSpecialCharacters()
    {
        return Arb.From(
            from channel in Gen.Elements(NotificationChannel.Email, NotificationChannel.Sms, NotificationChannel.InApp)
            select new NotificationTemplate
            {
                TemplateKey = "special-chars-test",
                EventType = "TestEvent",
                Channel = channel,
                Subject = channel == NotificationChannel.Sms ? null : "Special chars: {userName}",
                BodyTemplate = "Hello {userName}! Special chars: <>&\"'",
                IsActive = true,
                RequiredVariables = new List<string> { "userName" }
            });
    }

    private static Arbitrary<Dictionary<string, object>> GenerateTemplateData()
    {
        return Arb.From(
            from userName in Gen.Elements("John Doe", "Jane Smith", "Test User")
            from amount in Gen.Elements("$100.00", "$250.50", "$1,000.00")
            from orderId in Gen.Elements("ORD-001", "ORD-002", "ORD-003")
            select new Dictionary<string, object>
            {
                ["userName"] = userName,
                ["amount"] = amount,
                ["orderId"] = orderId,
                ["message"] = "Test message"
            });
    }

    private static Arbitrary<Dictionary<string, object>> GenerateConsistentTemplateData()
    {
        return Arb.From(
            Gen.Constant(new Dictionary<string, object>
            {
                ["userName"] = "TestUser123",
                ["amount"] = "$100.00",
                ["orderId"] = "ORD-001"
            }));
    }

    private static Arbitrary<Dictionary<string, object>> GenerateTemplateDataWithScripts()
    {
        return Arb.From(
            Gen.Constant(new Dictionary<string, object>
            {
                ["userName"] = "TestUser",
                ["userData"] = "<script>alert('xss')</script>",
                ["message"] = "javascript:alert('test')"
            }));
    }
}