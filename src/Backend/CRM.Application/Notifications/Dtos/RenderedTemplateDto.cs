namespace CRM.Application.Notifications.DTOs;

public class RenderedTemplateDto
{
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
}