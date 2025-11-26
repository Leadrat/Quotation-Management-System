using CRM.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Commands;
using CRM.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.Commands.Handlers
{
    public class MarkNotificationsReadCommandHandler : IRequestHandler<MarkNotificationsReadCommand, int>
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<MarkNotificationsReadCommandHandler> _logger;

        public MarkNotificationsReadCommandHandler(
            IAppDbContext db,
            ILogger<MarkNotificationsReadCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<int> Handle(MarkNotificationsReadCommand command, CancellationToken cancellationToken)
        {
            IQueryable<UserNotification> query = _db.Notifications
                .Where(n => n.RecipientUserId == command.RequestedByUserId && !n.IsRead);

            // If specific IDs provided, filter by them; otherwise mark all unread
            if (command.NotificationIds != null && command.NotificationIds.Any())
            {
                query = query.Where(n => command.NotificationIds.Contains(n.NotificationId));
            }

            var notifications = await query.ToListAsync(cancellationToken);
            var count = 0;

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTimeOffset.UtcNow;
                notification.UpdatedAt = DateTimeOffset.UtcNow;
                count++;

                // Publish domain event (simplified - in production, use event bus)
                var evt = new NotificationRead
                {
                    NotificationId = notification.NotificationId,
                    UserId = command.RequestedByUserId,
                    ReadAt = notification.ReadAt ?? DateTimeOffset.UtcNow
                };
                _logger.LogInformation("Notification {NotificationId} marked as read by user {UserId}",
                    notification.NotificationId, command.RequestedByUserId);
            }

            if (count > 0)
            {
                await _db.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Marked {Count} notifications as read for user {UserId}", count, command.RequestedByUserId);
            return count;
        }
    }
}

