using FsCheck;
using FsCheck.Xunit;
using Moq;
using CRM.Application.Notifications.DTOs;
using CRM.Application.Notifications.Repositories;
using CRM.Application.Notifications.Services;
using CRM.Infrastructure.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace CRM.Tests.Unit.Notifications;

/// <summary>
/// **Feature: notification-creation-dispatch, Property 8: Dynamic configuration management**
/// **Validates: Requirements 5.1, 5.2, 5.3, 5.4, 5.5**
/// </summary>
public class NotificationTemplatePropertyTests
{
    private readonly Mock<INotificationTemplateRepository> _mockRepository;
    private readonly INotificationTemplateValidationService _validationService;

    public NotificationTemplatePropertyTests()
    {
        _mockRepository = new Mock<INotificationTemplateRepository>();
        _validationService = new NotificationTemplateValidationService(_mockRepository.Object);
    }

    [Property(MaxTest = 100)]
    public Property ValidTemplateKeysShouldPassValidation()
    {
        var validTemplateKeyGen = from length in Gen.Choose(1, 100)
                                 from chars in Gen.ArrayOf(length, Gen.Elements("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-".ToCharArray()))
                                 where chars.Length > 0
                                 select new string(chars);

        return Prop.ForAll(validTemplateKeyGen, templateKey =>
        {
            var result = _validationService.ValidateTemplateKey(templateKey);
            return result.IsValid.Label($"Template key '{templateKey}' should be valid");
        });
    }

    [Property(MaxTest = 100)]
    public Property InvalidTemplateKeysShouldFailValidation()
    {
        var invalidTemplateKeyGen = Gen.OneOf(
            Gen.Constant(""), // Empty string
            Gen.Constant("   "), // Whitespace only
            from chars in Gen.NonEmptyListOf(Gen.Elements("!@#$%^&*()+=[]{}|\\:;\"'<>?,./".ToCharArray()))
                select new string(chars.ToArray()), // Invalid characters
            from length in Gen.Choose(101, 200)
            from chars in Gen.ArrayOf(length, Gen.Elements("abcdefghijklmnopqrstuvwxyz".ToCharArray()))
            select new string(chars) // Too long
        );

        return Prop.ForAll(invalidTemplateKeyGen, templateKey =>
        {
            var result = _validationService.ValidateTemplateKey(templateKey);
            return (!result.IsValid).Label($"Template key '{templateKey}' should be invalid");
        });
    }

    [Property(MaxTest = 100)]
    public Property TemplateVariableValidationShouldBeConsistent()
    {
        var variableGen = Gen.Elements(new[] { "UserName", "QuotationNumber", "Amount", "ClientName", "Date" });
        var templateVariablesGen = Gen.ListOf(variableGen).Select(vars => vars.Distinct().ToList());
        
        return Prop.ForAll(templateVariablesGen, variables =>
        {
            // Create template that uses all declared variables
            var bodyTemplate = string.Join(" ", variables.Select(v => $"Hello {{{{ {v} }}}}"));
            
            var result = _validationService.ValidateTemplateVariables(bodyTemplate, variables);
            
            return result.IsValid.Label($"Template with variables {string.Join(", ", variables)} should be valid");
        });
    }

    [Property(MaxTest = 100)]
    public Property UndeclaredVariablesShouldFailValidation()
    {
        var declaredVariablesGen = Gen.ListOf(Gen.Elements(new[] { "UserName", "QuotationNumber" }))
                                     .Select(vars => vars.Distinct().ToList());
        var undeclaredVariableGen = Gen.Elements(new[] { "UndeclaredVar", "MissingVar", "UnknownVar" });

        return Prop.ForAll(declaredVariablesGen, undeclaredVariableGen, (declaredVars, undeclaredVar) =>
        {
            // Ensure the undeclared variable is not in the declared list
            if (declaredVars.Contains(undeclaredVar))
                return true.Label("Skipping test where undeclared variable is actually declared");

            var bodyTemplate = $"Hello {{{{ {undeclaredVar} }}}}";
            
            var result = _validationService.ValidateTemplateVariables(bodyTemplate, declaredVars);
            
            return (!result.IsValid).Label($"Template using undeclared variable '{undeclaredVar}' should fail validation");
        });
    }

    [Property(MaxTest = 100)]
    public Property CreateTemplateRequestValidationShouldRejectInvalidData()
    {
        var invalidRequestGen = Gen.OneOf(
            // Empty name
            Gen.Constant(new CreateNotificationTemplateRequest 
            { 
                TemplateKey = "valid_key", 
                Name = "", 
                BodyTemplate = "Valid body",
                Channel = NotificationChannel.Email
            }),
            // Empty body template
            Gen.Constant(new CreateNotificationTemplateRequest 
            { 
                TemplateKey = "valid_key", 
                Name = "Valid Name", 
                BodyTemplate = "",
                Channel = NotificationChannel.Email
            }),
            // Invalid template key
            Gen.Constant(new CreateNotificationTemplateRequest 
            { 
                TemplateKey = "invalid key with spaces!", 
                Name = "Valid Name", 
                BodyTemplate = "Valid body",
                Channel = NotificationChannel.Email
            })
        );

        return Prop.ForAll(invalidRequestGen, async request =>
        {
            _mockRepository.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            
            var result = await _validationService.ValidateCreateRequestAsync(request);
            
            return (!result.IsValid).Label($"Invalid create request should fail validation");
        });
    }

    [Property(MaxTest = 100)]
    public Property ValidCreateTemplateRequestShouldPassValidation()
    {
        var validNameGen = Gen.Elements(new[] { "Email Template", "SMS Template", "In-App Template" });
        var validKeyGen = Gen.Elements(new[] { "email_template", "sms_template", "inapp_template" });
        var validBodyGen = Gen.Elements(new[] { "Hello {{UserName}}", "Your quotation {{QuotationNumber}} is ready" });
        var channelGen = Gen.Elements(Enum.GetValues<NotificationChannel>());

        var validRequestGen = from name in validNameGen
                             from key in validKeyGen
                             from body in validBodyGen
                             from channel in channelGen
                             select new CreateNotificationTemplateRequest
                             {
                                 TemplateKey = key,
                                 Name = name,
                                 BodyTemplate = body,
                                 Channel = channel,
                                 Variables = ExtractVariablesFromTemplate(body)
                             };

        return Prop.ForAll(validRequestGen, async request =>
        {
            _mockRepository.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            
            var result = await _validationService.ValidateCreateRequestAsync(request);
            
            return result.IsValid.Label($"Valid create request should pass validation");
        });
    }

    [Property(MaxTest = 100)]
    public Property DuplicateTemplateKeyShouldFailValidation()
    {
        var templateKeyGen = Gen.Elements(new[] { "existing_template", "duplicate_key", "used_template" });

        return Prop.ForAll(templateKeyGen, async templateKey =>
        {
            _mockRepository.Setup(r => r.ExistsAsync(templateKey)).ReturnsAsync(true);
            
            var request = new CreateNotificationTemplateRequest
            {
                TemplateKey = templateKey,
                Name = "Valid Name",
                BodyTemplate = "Valid body",
                Channel = NotificationChannel.Email
            };
            
            var result = await _validationService.ValidateCreateRequestAsync(request);
            
            return (!result.IsValid && result.Errors.Any(e => e.Contains("already exists")))
                .Label($"Duplicate template key '{templateKey}' should fail validation");
        });
    }

    private static List<string> ExtractVariablesFromTemplate(string template)
    {
        var matches = Regex.Matches(template, @"\{\{(\w+)\}\}");
        return matches.Cast<Match>()
                     .Select(m => m.Groups[1].Value)
                     .Distinct()
                     .ToList();
    }
}