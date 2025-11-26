using CRM.Domain.Entities;
using System.Threading.Tasks;
using CRM.Domain.Events;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Refunds.EventHandlers
{
    public class AdjustmentApprovedEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly IEmailNotificationService _emailService;
        private readonly ILogger<AdjustmentApprovedEventHandler> _logger;

        public AdjustmentApprovedEventHandler(
            IAppDbContext db,
            IEmailNotificationService emailService,
            ILogger<AdjustmentApprovedEventHandler> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(AdjustmentApproved evt)
        {
            // Get adjustment to find requester
            var adjustment = await _db.Adjustments.FindAsync(evt.AdjustmentId);
            if (adjustment != null)
            {
                var requester = await _db.Users.FindAsync(adjustment.RequestedByUserId);
                if (requester != null)
                {
                    var notification = new UserNotification
                    {
                        NotificationId = System.Guid.NewGuid(),
                        RecipientUserId = requester.UserId,
                        RelatedEntityType = "Adjustment",
                        RelatedEntityId = evt.AdjustmentId,
                        EventType = "APPROVED",
                        Message = $"Your adjustment request for quotation {evt.QuotationId} has been approved.",
                        IsRead = false,
                        CreatedAt = System.DateTimeOffset.UtcNow
                    };

                    _db.Notifications.Add(notification);
                    await _emailService.SendEmailNotificationAsync(notification, requester);
                }
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("Adjustment approved event handled for adjustment {AdjustmentId}", evt.AdjustmentId);
        }
    }
}

