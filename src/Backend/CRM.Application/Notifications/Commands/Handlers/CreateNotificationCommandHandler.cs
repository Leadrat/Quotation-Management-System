using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Commands;
using CRM.Application.Notifications.Dtos;
using CRM.Application.Notifications.Exceptions;
using CRM.Domain.Entities;
using CRM.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Notifications.Commands.Handlers;

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, NotificationDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public CreateNotificationCommandHandler(IAppDbContext context, IMapper mapper, IMediator mediator)
    {
        _context = context;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<NotificationDto> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        // Validate that the user exists
        var userExists = await _context.Users
            .AnyAsync(u => u.UserId == request.UserId, cancellationToken);
        
        if (!userExists)
        {
            throw new ArgumentException($"User with ID {request.UserId} does not exist.");
        }

        // Validate that the notification type exists
        var notificationTypeExists = await _context.NotificationTypes
            .AnyAsync(nt => nt.NotificationTypeId == request.NotificationTypeId, cancellationToken);
        
        if (!notificationTypeExists)
        {
            throw new ArgumentException($"Notification type with ID {request.NotificationTypeId} does not exist.");
        }

        // Validate RelatedEntityType is provided if RelatedEntityId is provided
        if (request.RelatedEntityId.HasValue && string.IsNullOrWhiteSpace(request.RelatedEntityType))
        {
            throw new ArgumentException("RelatedEntityType is required when RelatedEntityId is provided.");
        }

        // Create the notification
        var notification = new UserNotification
        {
            NotificationId = Guid.NewGuid(),
            UserId = request.UserId,
            RecipientUserId = request.UserId, // Set both for compatibility
            NotificationTypeId = request.NotificationTypeId,
            EventType = "MANUAL", // Default event type for manually created notifications
            Title = request.Title,
            Message = request.Message,
            RelatedEntityId = request.RelatedEntityId,
            RelatedEntityType = request.RelatedEntityType,
            IsRead = false,
            ReadAt = null,
            IsArchived = false,
            SentVia = request.SentVia,
            DeliveryStatus = "SENT",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        // Load the notification with its type for mapping
        var createdNotification = await _context.Notifications
            .Include(n => n.NotificationType)
            .FirstAsync(n => n.NotificationId == notification.NotificationId, cancellationToken);

        // Publish domain event
        await _mediator.Publish(new NotificationCreated
        {
            NotificationId = notification.NotificationId,
            UserId = notification.UserId,
            NotificationTypeId = notification.NotificationTypeId,
            Title = notification.Title,
            Message = notification.Message,
            SentVia = notification.SentVia,
            CreatedAt = notification.CreatedAt
        }, cancellationToken);

        return _mapper.Map<NotificationDto>(createdNotification);
    }
}
