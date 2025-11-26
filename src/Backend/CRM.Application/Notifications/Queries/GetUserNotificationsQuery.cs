using CRM.Application.Common.Results;
using CRM.Application.Notifications.Dtos;
using MediatR;

namespace CRM.Application.Notifications.Queries;

public class GetUserNotificationsQuery : IRequest<PagedResult<NotificationDto>>
{
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool? IsRead { get; set; }
    public Guid? NotificationTypeId { get; set; }
    public DateTimeOffset? FromDate { get; set; }
    public DateTimeOffset? ToDate { get; set; }
    public bool IsAdmin { get; set; } // Admin can view all notifications
    public Guid? TargetUserId { get; set; } // Admin can specify target user
}
