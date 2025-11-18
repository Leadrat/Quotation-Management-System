using System.Threading.Tasks;
using CRM.Domain.Events;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Refunds.EventHandlers
{
    public class RefundCompletedEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly IEmailNotificationService _emailService;
        private readonly ILogger<RefundCompletedEventHandler> _logger;

        public RefundCompletedEventHandler(
            IAppDbContext db,
            IEmailNotificationService emailService,
            ILogger<RefundCompletedEventHandler> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(RefundCompleted evt)
        {
            // Get refund to find requester
            var refund = await _db.Refunds.FindAsync(evt.RefundId);
            if (refund != null)
            {
                var requester = await _db.Users.FindAsync(refund.RequestedByUserId);
                if (requester != null)
                {
                    var notification = new Domain.Entities.Notification
                    {
                        NotificationId = System.Guid.NewGuid(),
                        RecipientUserId = requester.UserId,
                        RelatedEntityType = "Refund",
                        RelatedEntityId = evt.RefundId,
                        EventType = "COMPLETED",
                        Message = $"Your refund of {evt.RefundAmount:C} has been successfully processed.",
                        IsRead = false,
                        CreatedAt = System.DateTimeOffset.UtcNow
                    };

                    _db.Notifications.Add(notification);
                    await _emailService.SendEmailNotificationAsync(notification, requester);
                }
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("Refund completed event handled for refund {RefundId}", evt.RefundId);
        }
    }
}

