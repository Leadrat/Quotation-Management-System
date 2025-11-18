using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Commands;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.Commands.Handlers
{
    public class ArchiveNotificationsCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<ArchiveNotificationsCommandHandler> _logger;

        public ArchiveNotificationsCommandHandler(
            IAppDbContext db,
            ILogger<ArchiveNotificationsCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<int> Handle(ArchiveNotificationsCommand command)
        {
            IQueryable<Domain.Entities.Notification> query = _db.Notifications
                .Where(n => n.RecipientUserId == command.RequestedByUserId && !n.IsArchived);

            // If specific IDs provided, filter by them; otherwise archive all
            if (command.NotificationIds != null && command.NotificationIds.Any())
            {
                query = query.Where(n => command.NotificationIds.Contains(n.NotificationId));
            }

            var notifications = await query.ToListAsync();
            var count = 0;

            foreach (var notification in notifications)
            {
                notification.Archive();
                count++;

                // Publish domain event
                var evt = new NotificationArchived
                {
                    NotificationId = notification.NotificationId,
                    UserId = command.RequestedByUserId,
                    ArchivedAt = notification.ArchivedAt ?? DateTimeOffset.UtcNow
                };
                _logger.LogInformation("Notification {NotificationId} archived by user {UserId}",
                    notification.NotificationId, command.RequestedByUserId);
            }

            if (count > 0)
            {
                await _db.SaveChangesAsync();
            }

            _logger.LogInformation("Archived {Count} notifications for user {UserId}", count, command.RequestedByUserId);
            return count;
        }
    }
}

