using AutoMapper;
using CRM.Application.Notifications.DTOs;
using CRM.Application.Notifications.Repositories;
using CRM.Application.Notifications.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CRM.Infrastructure.Services;

public class NotificationTemplateService : INotificationTemplateService
{
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly INotificationTemplateValidationService _validationService;
    private readonly IMapper _mapper;
    private static readonly Regex VariablePattern = new(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

    public NotificationTemplateService(
        INotificationTemplateRepository templateRepository,
        INotificationTemplateValidationService validationService,
        IMapper mapper)
    {
        _templateRepository = templateRepository;
        _validationService = validationService;
        _mapper = mapper;
    }

    public async Task<NotificationTemplate?> GetTemplateAsync(string templateKey)
    {
        return await _templateRepository.GetByTemplateKeyAsync(templateKey);
    }

    public async Task<RenderedTemplateDto> RenderTemplateAsync(string templateKey, object data, NotificationChannel channel)
    {
        var template = await _templateRepository.GetByTemplateKeyAsync(templateKey);
        if (template == null)
        {
            throw new InvalidOperationException($"Template with key '{templateKey}' not found");
        }

        if (template.Channel != channel)
        {
            throw new InvalidOperationException($"Template '{templateKey}' is not configured for channel '{channel}'");
        }

        if (!template.IsActive)
        {
            throw new InvalidOperationException($"Template '{templateKey}' is not active");
        }

        var renderedContent = RenderTemplate(template.BodyTemplate, data);
        var renderedSubject = RenderTemplate(template.Subject, data);
        
        // Apply channel-specific formatting
        var formattedContent = ApplyChannelSpecificFormatting(renderedContent, channel, renderedSubject, data);
        
        return new RenderedTemplateDto
        {
            Subject = renderedSubject,
            Body = formattedContent
        };
    }

    public async Task<NotificationTemplateDto> CreateTemplateAsync(CreateNotificationTemplateRequest request)
    {
        var validation = await _validationService.ValidateCreateRequestAsync(request);
        if (!validation.IsValid)
        {
            throw new ArgumentException($"Template validation failed: {string.Join(", ", validation.Errors)}");
        }

        var template = _mapper.Map<NotificationTemplate>(request);
        var createdTemplate = await _templateRepository.CreateAsync(template);
        
        return _mapper.Map<NotificationTemplateDto>(createdTemplate);
    }

    public async Task<NotificationTemplateDto> UpdateTemplateAsync(int templateId, UpdateNotificationTemplateRequest request)
    {
        var validation = await _validationService.ValidateUpdateRequestAsync(templateId, request);
        if (!validation.IsValid)
        {
            throw new ArgumentException($"Template validation failed: {string.Join(", ", validation.Errors)}");
        }

        var existingTemplate = await _templateRepository.GetByIdAsync(templateId);
        if (existingTemplate == null)
        {
            throw new InvalidOperationException($"Template with ID {templateId} not found");
        }

        _mapper.Map(request, existingTemplate);
        var updatedTemplate = await _templateRepository.UpdateAsync(existingTemplate);
        
        return _mapper.Map<NotificationTemplateDto>(updatedTemplate);
    }

    public async Task DeleteTemplateAsync(int templateId)
    {
        await _templateRepository.DeleteAsync(templateId);
    }

    public async Task<List<NotificationTemplateDto>> GetTemplatesAsync(NotificationChannel? channel = null, bool activeOnly = true)
    {
        List<NotificationTemplate> templates;
        
        if (channel.HasValue)
        {
            templates = await _templateRepository.GetByChannelAsync(channel.Value, activeOnly);
        }
        else
        {
            templates = await _templateRepository.GetAllAsync(activeOnly);
        }

        return _mapper.Map<List<NotificationTemplateDto>>(templates);
    }

    public async Task<List<NotificationTemplateDto>> SearchTemplatesAsync(string searchTerm, NotificationChannel? channel = null, bool activeOnly = true)
    {
        var templates = await _templateRepository.SearchAsync(searchTerm, channel, activeOnly);
        return _mapper.Map<List<NotificationTemplateDto>>(templates);
    }

    public async Task<bool> ValidateTemplateAsync(string templateKey, object sampleData)
    {
        try
        {
            var template = await _templateRepository.GetByTemplateKeyAsync(templateKey);
            if (template == null)
            {
                return false;
            }

            // Try to render the template with sample data
            RenderTemplate(template.BodyTemplate, sampleData);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string RenderTemplate(string template, object data)
    {
        if (string.IsNullOrEmpty(template))
        {
            return string.Empty;
        }

        var dataDict = ConvertObjectToDictionary(data);
        
        return VariablePattern.Replace(template, match =>
        {
            var variableName = match.Groups[1].Value;
            
            if (dataDict.TryGetValue(variableName, out var value))
            {
                return value?.ToString() ?? string.Empty;
            }
            
            // Return the original placeholder if variable not found
            return match.Value;
        });
    }

    private static Dictionary<string, object?> ConvertObjectToDictionary(object data)
    {
        if (data is Dictionary<string, object?> dict)
        {
            return dict;
        }

        var result = new Dictionary<string, object?>();
        
        if (data != null)
        {
            var properties = data.GetType().GetProperties();
            foreach (var property in properties)
            {
                result[property.Name] = property.GetValue(data);
            }
        }

        return result;
    }

    private static string ApplyChannelSpecificFormatting(string content, NotificationChannel channel, string subject, object data)
    {
        return channel switch
        {
            NotificationChannel.InApp => FormatForInApp(content),
            NotificationChannel.Email => FormatForEmail(content, subject, data),
            NotificationChannel.SMS => FormatForSms(content),
            _ => content
        };
    }

    private static string FormatForInApp(string content)
    {
        // In-app notifications: Keep content as-is, optimized for real-time display
        return content.Trim();
    }

    private static string FormatForEmail(string content, string subject, object data)
    {
        // Email notifications: Wrap in HTML structure with subject
        var renderedSubject = RenderTemplate(subject, data);
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>{renderedSubject}</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin-bottom: 20px; }}
        .content {{ padding: 15px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>{renderedSubject}</h2>
        </div>
        <div class=""content"">
            {content}
        </div>
    </div>
</body>
</html>";
    }

    private static string FormatForSms(string content)
    {
        // SMS notifications: Limit to 160 characters and remove HTML
        var plainText = System.Text.RegularExpressions.Regex.Replace(content, "<.*?>", string.Empty);
        plainText = plainText.Trim();
        
        if (plainText.Length > 160)
        {
            plainText = plainText.Substring(0, 157) + "...";
        }
        
        return plainText;
    }
}