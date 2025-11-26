using CRM.Domain.Entities;
using System.Threading.Tasks;
using CRM.Domain.Events;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Refunds.EventHandlers
{
    public class RefundFailedEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly IEmailNotificationService _emailService;
        private readonly ILogger<RefundFailedEventHandler> _logger;

        public RefundFailedEventHandler(
            IAppDbContext db,
            IEmailNotificationService emailService,
            ILogger<RefundFailedEventHandler> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(RefundFailed evt)
        {
            // Get refund to find requester
            var refund = await _db.Refunds.FindAsync(evt.RefundId);
            if (refund != null)
            {
                var requester = await _db.Users.FindAsync(refund.RequestedByUserId);
                if (requester != null)
                {
                    var notification = new UserNotification
                    {
                        NotificationId = System.Guid.NewGuid(),
                        RecipientUserId = requester.UserId,
                        RelatedEntityType = "Refund",
                        RelatedEntityId = evt.RefundId,
                        EventType = "FAILED",
                        Message = $"Your refund request failed. Reason: {evt.FailureReason}",
                        IsRead = false,
                        CreatedAt = System.DateTimeOffset.UtcNow
                    };

                    _db.Notifications.Add(notification);
                    await _emailService.SendEmailNotificationAsync(notification, requester);
                }
            }

            await _db.SaveChangesAsync();
            _logger.LogError("Refund failed event handled for refund {RefundId}: {Reason}", evt.RefundId, evt.FailureReason);
        }
    }
}

