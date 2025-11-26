using MediatR;

namespace CRM.Domain.Events;

public class NotificationRead : INotification
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset ReadAt { get; set; }
}