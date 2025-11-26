using CRM.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.Events;

public class NotificationReadEventHandler : INotificationHandler<NotificationRead>
{
    private readonly ILogger<NotificationReadEventHandler> _logger;

    public NotificationReadEventHandler(ILogger<NotificationReadEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(NotificationRead notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Notification marked as read: {NotificationId} by user {UserId} at {ReadAt}",
            notification.NotificationId,
            notification.UserId,
            notification.ReadAt);

        // Here you could add additional logic like:
        // - Updating real-time UI via SignalR
        // - Updating analytics/metrics
        // - Triggering follow-up actions

        return Task.CompletedTask;
    }
}
