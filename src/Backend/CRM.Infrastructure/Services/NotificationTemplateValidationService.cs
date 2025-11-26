using System.Text.RegularExpressions;
using CRM.Application.Notifications.DTOs;
using CRM.Application.Notifications.Repositories;
using CRM.Application.Notifications.Services;

namespace CRM.Infrastructure.Services;

public class NotificationTemplateValidationService : INotificationTemplateValidationService
{
    private readonly INotificationTemplateRepository _templateRepository;
    private static readonly Regex VariablePattern = new(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

    public NotificationTemplateValidationService(INotificationTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<ValidationResult> ValidateCreateRequestAsync(CreateNotificationTemplateRequest request)
    {
        var errors = new List<string>();

        // Validate template key
        var keyValidation = ValidateTemplateKey(request.TemplateKey);
        if (!keyValidation.IsValid)
        {
            errors.AddRange(keyValidation.Errors);
        }

        // Check if template key already exists
        if (await _templateRepository.ExistsAsync(request.TemplateKey))
        {
            errors.Add($"Template with key '{request.TemplateKey}' already exists");
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add("Template name is required");
        }

        if (string.IsNullOrWhiteSpace(request.BodyTemplate))
        {
            errors.Add("Template body is required");
        }

        // Validate template variables
        if (!string.IsNullOrWhiteSpace(request.BodyTemplate))
        {
            var variableValidation = ValidateTemplateVariables(request.BodyTemplate, request.Variables);
            if (!variableValidation.IsValid)
            {
                errors.AddRange(variableValidation.Errors);
            }
        }

        return errors.Any() ? ValidationResult.Failure(errors.ToArray()) : ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateUpdateRequestAsync(int templateId, UpdateNotificationTemplateRequest request)
    {
        var errors = new List<string>();

        // Check if template exists
        var existingTemplate = await _templateRepository.GetByIdAsync(templateId);
        if (existingTemplate == null)
        {
            errors.Add($"Template with ID {templateId} not found");
            return ValidationResult.Failure(errors.ToArray());
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add("Template name is required");
        }

        if (string.IsNullOrWhiteSpace(request.BodyTemplate))
        {
            errors.Add("Template body is required");
        }

        // Validate template variables
        if (!string.IsNullOrWhiteSpace(request.BodyTemplate))
        {
            var variableValidation = ValidateTemplateVariables(request.BodyTemplate, request.Variables);
            if (!variableValidation.IsValid)
            {
                errors.AddRange(variableValidation.Errors);
            }
        }

        return errors.Any() ? ValidationResult.Failure(errors.ToArray()) : ValidationResult.Success();
    }

    public ValidationResult ValidateTemplateVariables(string bodyTemplate, List<string> declaredVariables)
    {
        var errors = new List<string>();

        // Extract variables from template
        var matches = VariablePattern.Matches(bodyTemplate);
        var usedVariables = matches.Cast<Match>()
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .ToList();

        // Check for undeclared variables
        var undeclaredVariables = usedVariables.Except(declaredVariables).ToList();
        if (undeclaredVariables.Any())
        {
            errors.Add($"Template uses undeclared variables: {string.Join(", ", undeclaredVariables)}");
        }

        // Check for unused declared variables
        var unusedVariables = declaredVariables.Except(usedVariables).ToList();
        if (unusedVariables.Any())
        {
            errors.Add($"Declared variables not used in template: {string.Join(", ", unusedVariables)}");
        }

        return errors.Any() ? ValidationResult.Failure(errors.ToArray()) : ValidationResult.Success();
    }

    public ValidationResult ValidateTemplateKey(string templateKey)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(templateKey))
        {
            errors.Add("Template key is required");
        }
        else
        {
            // Template key should be alphanumeric with underscores and hyphens
            if (!Regex.IsMatch(templateKey, @"^[a-zA-Z0-9_-]+$"))
            {
                errors.Add("Template key can only contain letters, numbers, underscores, and hyphens");
            }

            if (templateKey.Length > 100)
            {
                errors.Add("Template key cannot exceed 100 characters");
            }
        }

        return errors.Any() ? ValidationResult.Failure(errors.ToArray()) : ValidationResult.Success();
    }
}