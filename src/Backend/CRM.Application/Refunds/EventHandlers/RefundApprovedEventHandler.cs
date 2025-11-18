using System.Threading.Tasks;
using CRM.Domain.Events;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Refunds.EventHandlers
{
    public class RefundApprovedEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly IEmailNotificationService _emailService;
        private readonly ILogger<RefundApprovedEventHandler> _logger;

        public RefundApprovedEventHandler(
            IAppDbContext db,
            IEmailNotificationService emailService,
            ILogger<RefundApprovedEventHandler> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(RefundApproved evt)
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
                    EventType = "APPROVED",
                    Message = $"Your refund request of {evt.RefundAmount:C} has been approved and will be processed shortly.",
                    IsRead = false,
                    CreatedAt = System.DateTimeOffset.UtcNow
                };

                _db.Notifications.Add(notification);
                await _emailService.SendEmailNotificationAsync(notification, requester);
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("Refund approved event handled for refund {RefundId}", evt.RefundId);
        }
    }
}

