using CRM.Application.Notifications.DTOs;

namespace CRM.Application.Notifications.Services;

public interface INotificationTemplateValidationService
{
    Task<ValidationResult> ValidateCreateRequestAsync(CreateNotificationTemplateRequest request);
    Task<ValidationResult> ValidateUpdateRequestAsync(int templateId, UpdateNotificationTemplateRequest request);
    ValidationResult ValidateTemplateVariables(string bodyTemplate, List<string> declaredVariables);
    ValidationResult ValidateTemplateKey(string templateKey);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ValidationResult Success() => new() { IsValid = true };
    
    public static ValidationResult Failure(params string[] errors) => new() 
    { 
        IsValid = false, 
        Errors = errors.ToList() 
    };
}