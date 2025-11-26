using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CRM.Application.Common.Interfaces;
using CRM.Domain.Entities;
using CRM.Infrastructure.Services;
using CRM.Tests.Common;

namespace CRM.Tests.Notifications;

/**
 * Feature: notification-creation-dispatch, Property 8: Dynamic configuration management
 * Validates: Requirements 5.1, 5.2, 5.3, 5.4, 5.5
 */
public class TemplateManagementPropertyTests : BaseIntegrationTest
{
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly INotificationTemplateService _templateService;

    public TemplateManagementPropertyTests()
    {
        _templateRepository = ServiceProvider.GetRequiredService<INotificationTemplateRepository>();
        _templateService = ServiceProvider.GetRequiredService<INotificationTemplateService>();
    }

    [Property]
    public Property TemplateCreationAndRetrieval_ShouldBeConsistent()
    {
        return Prop.ForAll(
            GenerateValidTemplate(),
            async template =>
            {
                // Create template
                var created = await _templateRepository.CreateAsync(template);
                
                // Retrieve template
                var retrieved = await _templateRepository.GetByKeyAsync(template.TemplateKey);
                
                return retrieved != null &&
                       retrieved.TemplateKey == template.TemplateKey &&
                       retrieved.EventType == template.EventType &&
                       retrieved.Channel == template.Channel &&
                       retrieved.BodyTemplate == template.BodyTemplate &&
                       retrieved.IsActive == template.IsActive;
            });
    }

    [Property]
    public Property TemplateVariableValidation_ShouldEnforceRequiredVariables()
    {
        return Prop.ForAll(
            GenerateTemplateWithVariables(),
            GenerateTemplateData(),
            async (template, templateData) =>
            {
                await _templateRepository.CreateAsync(template);
                
                try
                {
                    var result = await _templateService.RenderTemplateAsync(template, templateData);
                    
                    // If rendering succeeds, all required variables must be present
                    return template.RequiredVariables.All(variable => 
                        templateData.ContainsKey(variable));
                }
                catch (ArgumentException)
                {
                    // If rendering fails, at least one required variable must be missing
                    return template.RequiredVariables.Any(variable => 
                        !templateData.ContainsKey(variable));
                }
            });
    }

    [Property]
    public Property TemplateUpdate_ShouldPreserveKey()
    {
        return Prop.ForAll(
            GenerateValidTemplate(),
            GenerateValidTemplate(),
            async (original, updated) =>
            {
                // Ensure same key for update
                updated.TemplateKey = original.TemplateKey;
                updated.Id = original.Id;
                
                await _templateRepository.CreateAsync(original);
                await _templateRepository.UpdateAsync(updated);
                
                var retrieved = await _templateRepository.GetByKeyAsync(original.TemplateKey);
                
                return retrieved != null &&
                       retrieved.TemplateKey == original.TemplateKey &&
                       retrieved.BodyTemplate == updated.BodyTemplate &&
                       retrieved.IsActive == updated.IsActive;
            });
    }

    [Property]
    public Property TemplateActivation_ShouldControlVisibility()
    {
        return Prop.ForAll(
            GenerateValidTemplate(),
            async template =>
            {
                await _templateRepository.CreateAsync(template);
                
                var activeTemplates = await _templateRepository.GetActiveTemplatesAsync();
                var allTemplates = await _templateRepository.GetByEventTypeAsync(template.EventType);
                
                if (template.IsActive)
                {
                    return activeTemplates.Any(t => t.TemplateKey == template.TemplateKey) &&
                           allTemplates.Any(t => t.TemplateKey == template.TemplateKey);
                }
                else
                {
                    return !activeTemplates.Any(t => t.TemplateKey == template.TemplateKey) &&
                           allTemplates.Any(t => t.TemplateKey == template.TemplateKey);
                }
            });
    }

    [Property]
    public Property ChannelSpecificRetrieval_ShouldFilterCorrectly()
    {
        return Prop.ForAll(
            GenerateTemplatesForAllChannels(),
            async templates =>
            {
                foreach (var template in templates)
                {
                    await _templateRepository.CreateAsync(template);
                }
                
                var eventType = templates.First().EventType;
                
                foreach (NotificationChannel channel in Enum.GetValues<NotificationChannel>())
                {
                    var channelTemplates = await _templateRepository.GetByChannelAsync(channel);
                    var eventChannelTemplate = await _templateRepository.GetByEventTypeAndChannelAsync(eventType, channel);
                    
                    var expectedTemplate = templates.FirstOrDefault(t => t.Channel == channel && t.IsActive);
                    
                    if (expectedTemplate != null)
                    {
                        if (!channelTemplates.Any(t => t.TemplateKey == expectedTemplate.TemplateKey) ||
                            eventChannelTemplate?.TemplateKey != expectedTemplate.TemplateKey)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (eventChannelTemplate != null)
                        {
                            return false;
                        }
                    }
                }
                
                return true;
            });
    }

    private static Arbitrary<NotificationTemplate> GenerateValidTemplate()
    {
        return Arb.From(
            from templateKey in Gen.Elements("quotation-approved", "payment-received", "approval-requested", "user-registered")
            from eventType in Gen.Elements("QuotationApproved", "PaymentReceived", "ApprovalRequested", "UserRegistered")
            from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.Sms)
            from subject in Gen.Elements("Test Subject", "Important Update", "Action Required")
            from body in Gen.Elements("Hello {userName}, your {itemType} is ready.", "Dear {userName}, please review {documentName}.")
            from isActive in Arb.Generate<bool>()
            from variables in Gen.ListOf(Gen.Elements("userName", "itemType", "documentName", "amount"))
            select new NotificationTemplate
            {
                TemplateKey = $"{templateKey}-{channel.ToString().ToLower()}",
                EventType = eventType,
                Channel = channel,
                Subject = subject,
                BodyTemplate = body,
                IsActive = isActive,
                RequiredVariables = variables.Distinct().ToList()
            });
    }

    private static Arbitrary<NotificationTemplate> GenerateTemplateWithVariables()
    {
        return Arb.From(
            from template in GenerateValidTemplate().Generator
            from variableCount in Gen.Choose(1, 5)
            from variables in Gen.ListOf(Gen.Elements("userName", "amount", "date", "itemName", "status"), variableCount)
            select new NotificationTemplate
            {
                TemplateKey = template.TemplateKey,
                EventType = template.EventType,
                Channel = template.Channel,
                Subject = template.Subject,
                BodyTemplate = template.BodyTemplate,
                IsActive = template.IsActive,
                RequiredVariables = variables.Distinct().ToList()
            });
    }

    private static Arbitrary<Dictionary<string, object>> GenerateTemplateData()
    {
        return Arb.From(
            from hasUserName in Arb.Generate<bool>()
            from hasAmount in Arb.Generate<bool>()
            from hasDate in Arb.Generate<bool>()
            from hasItemName in Arb.Generate<bool>()
            from hasStatus in Arb.Generate<bool>()
            select new Dictionary<string, object>
            {
                ["userName"] = hasUserName ? "John Doe" : null,
                ["amount"] = hasAmount ? "$100.00" : null,
                ["date"] = hasDate ? DateTime.Now.ToString() : null,
                ["itemName"] = hasItemName ? "Test Item" : null,
                ["status"] = hasStatus ? "Active" : null
            }.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    }

    private static Arbitrary<List<NotificationTemplate>> GenerateTemplatesForAllChannels()
    {
        return Arb.From(
            from eventType in Gen.Elements("TestEvent", "SampleEvent", "DemoEvent")
            from templates in Gen.ListOf(
                from channel in Gen.Elements(NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.Sms)
                from isActive in Arb.Generate<bool>()
                select new NotificationTemplate
                {
                    TemplateKey = $"{eventType.ToLower()}-{channel.ToString().ToLower()}",
                    EventType = eventType,
                    Channel = channel,
                    Subject = $"Test Subject for {channel}",
                    BodyTemplate = $"Test body for {channel}: {{userName}}",
                    IsActive = isActive,
                    RequiredVariables = new List<string> { "userName" }
                }, 3)
            select templates.Take(3).ToList());
    }
}