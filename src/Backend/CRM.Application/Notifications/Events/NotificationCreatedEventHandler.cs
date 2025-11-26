using CRM.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.Events;

public class NotificationCreatedEventHandler : INotificationHandler<NotificationCreated>
{
    private readonly ILogger<NotificationCreatedEventHandler> _logger;

    public NotificationCreatedEventHandler(ILogger<NotificationCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(NotificationCreated notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Notification created: {NotificationId} for user {UserId} of type {NotificationTypeId}",
            notification.NotificationId,
            notification.UserId,
            notification.NotificationTypeId);

        // Here you could add additional logic like:
        // - Sending real-time notifications via SignalR
        // - Triggering email/SMS delivery
        // - Updating analytics/metrics

        return Task.CompletedTask;
    }
}
