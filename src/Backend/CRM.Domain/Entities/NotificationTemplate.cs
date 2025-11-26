using CRM.Domain.Enums;

namespace CRM.Domain.Entities;

public class NotificationTemplate
{
    public int Id { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string EventType { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public string? Subject { get; set; }
    public string BodyTemplate { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<string>? RequiredVariables { get; set; } = new();
    public string? Variables { get; set; } // Template variables as JSON string
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<NotificationDispatchAttempt> DispatchAttempts { get; set; } = new List<NotificationDispatchAttempt>();
}