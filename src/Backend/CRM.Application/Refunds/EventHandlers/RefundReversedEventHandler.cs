using CRM.Domain.Entities;
using System.Threading.Tasks;
using CRM.Domain.Events;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Refunds.EventHandlers
{
    public class RefundReversedEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly IEmailNotificationService _emailService;
        private readonly ILogger<RefundReversedEventHandler> _logger;

        public RefundReversedEventHandler(
            IAppDbContext db,
            IEmailNotificationService emailService,
            ILogger<RefundReversedEventHandler> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(RefundReversed evt)
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
                        EventType = "REVERSED",
                        Message = $"Your refund has been reversed. Reason: {evt.ReversedReason}",
                        IsRead = false,
                        CreatedAt = System.DateTimeOffset.UtcNow
                    };

                    _db.Notifications.Add(notification);
                    await _emailService.SendEmailNotificationAsync(notification, requester);
                }
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("Refund reversed event handled for refund {RefundId}", evt.RefundId);
        }
    }
}

