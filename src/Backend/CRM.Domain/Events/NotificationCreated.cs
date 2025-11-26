using MediatR;

namespace CRM.Domain.Events;

public class NotificationCreated : INotification
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public Guid NotificationTypeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string SentVia { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}