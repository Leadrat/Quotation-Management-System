using CRM.Application.Notifications.Dtos;
using MediatR;

namespace CRM.Application.Notifications.Commands;

public class MarkNotificationAsReadCommand : IRequest<NotificationDto>
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; } // For authorization
    public bool IsAdmin { get; set; } // Admin can mark any notification as read
}
