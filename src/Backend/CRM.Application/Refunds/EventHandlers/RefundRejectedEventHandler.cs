using System.Threading.Tasks;
using CRM.Domain.Events;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Refunds.EventHandlers
{
    public class RefundRejectedEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly IEmailNotificationService _emailService;
        private readonly ILogger<RefundRejectedEventHandler> _logger;

        public RefundRejectedEventHandler(
            IAppDbContext db,
            IEmailNotificationService emailService,
            ILogger<RefundRejectedEventHandler> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(RefundRejected evt)
        {
            // Get requester
            var requester = await _db.Users.FindAsync(evt.RequestedByUserId);
            if (requester != null)
            {
                var notification = new Domain.Entities.Notification
                {
                    NotificationId = System.Guid.NewGuid(),
                    RecipientUserId = requester.UserId,
                    RelatedEntityType = "Refund",
                    RelatedEntityId = evt.RefundId,
                    EventType = "REJECTED",
                    Message = $"Your refund request has been rejected. Reason: {evt.RejectionReason}",
                    IsRead = false,
                    CreatedAt = System.DateTimeOffset.UtcNow
                };

                _db.Notifications.Add(notification);
                await _emailService.SendEmailNotificationAsync(notification, requester);
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("Refund rejected event handled for refund {RefundId}", evt.RefundId);
        }
    }
}

