using MediatR;

namespace CRM.Application.Notifications.Queries;

public class GetUnreadNotificationCountQuery : IRequest<int>
{
    public Guid UserId { get; set; }
}
