using CRM.Application.Notifications.Dtos;
using MediatR;

namespace CRM.Application.Notifications.Commands;

public class CreateNotificationCommand : IRequest<NotificationDto>
{
    public Guid UserId { get; set; }
    public Guid NotificationTypeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public string SentVia { get; set; } = string.Empty;
}
