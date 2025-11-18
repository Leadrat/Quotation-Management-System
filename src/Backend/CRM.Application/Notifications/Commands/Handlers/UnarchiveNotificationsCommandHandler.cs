using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.Commands.Handlers
{
    public class UnarchiveNotificationsCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<UnarchiveNotificationsCommandHandler> _logger;

        public UnarchiveNotificationsCommandHandler(
            IAppDbContext db,
            ILogger<UnarchiveNotificationsCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<int> Handle(UnarchiveNotificationsCommand command)
        {
            if (command.NotificationIds == null || !command.NotificationIds.Any())
            {
                throw new ArgumentException("NotificationIds must be provided and non-empty");
            }

            var notifications = await _db.Notifications
                .Where(n => n.RecipientUserId == command.RequestedByUserId &&
                           n.IsArchived &&
                           command.NotificationIds.Contains(n.NotificationId))
                .ToListAsync();

            var count = 0;

            foreach (var notification in notifications)
            {
                notification.Unarchive();
                count++;
                _logger.LogInformation("Notification {NotificationId} unarchived by user {UserId}",
                    notification.NotificationId, command.RequestedByUserId);
            }

            if (count > 0)
            {
                await _db.SaveChangesAsync();
            }

            _logger.LogInformation("Unarchived {Count} notifications for user {UserId}", count, command.RequestedByUserId);
            return count;
        }
    }
}

