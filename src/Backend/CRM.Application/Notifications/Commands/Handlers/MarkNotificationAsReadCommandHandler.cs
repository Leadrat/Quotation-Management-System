using CRM.Domain.Entities;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Commands;
using CRM.Application.Notifications.Dtos;
using CRM.Application.Notifications.Exceptions;
using CRM.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Notifications.Commands.Handlers;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, NotificationDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public MarkNotificationAsReadCommandHandler(IAppDbContext context, IMapper mapper, IMediator mediator)
    {
        _context = context;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<NotificationDto> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        // Find the notification
        var notification = await _context.Notifications
            .Include(n => n.NotificationType)
            .FirstOrDefaultAsync(n => n.NotificationId == request.NotificationId, cancellationToken);

        if (notification == null)
        {
            throw new NotificationNotFoundException(request.NotificationId);
        }

        // Check authorization - user can only mark their own notifications as read
        // Admin can mark any notification as read
        if (notification.UserId != request.UserId && !request.IsAdmin)
        {
            throw new UnauthorizedNotificationAccessException(request.NotificationId, request.UserId);
        }

        // Update read status if not already read
        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTimeOffset.UtcNow;
            notification.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Publish domain event
            await _mediator.Publish(new NotificationRead
            {
                NotificationId = notification.NotificationId,
                UserId = notification.UserId,
                ReadAt = notification.ReadAt.Value
            }, cancellationToken);
        }

        return _mapper.Map<NotificationDto>(notification);
    }
}
