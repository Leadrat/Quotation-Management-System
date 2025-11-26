using CRM.Domain.Entities;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Notifications.Queries.Handlers;

public class GetUnreadNotificationCountQueryHandler : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    private readonly IAppDbContext _context;

    public GetUnreadNotificationCountQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _context.Notifications
            .Where(n => n.UserId == request.UserId && !n.IsRead)
            .CountAsync(cancellationToken);

        return count;
    }
}
