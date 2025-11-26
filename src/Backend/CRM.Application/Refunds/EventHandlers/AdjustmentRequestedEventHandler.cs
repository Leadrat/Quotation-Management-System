using CRM.Domain.Entities;
using System.Threading.Tasks;
using CRM.Domain.Events;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Refunds.EventHandlers
{
    public class AdjustmentRequestedEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly IEmailNotificationService _emailService;
        private readonly ILogger<AdjustmentRequestedEventHandler> _logger;

        public AdjustmentRequestedEventHandler(
            IAppDbContext db,
            IEmailNotificationService emailService,
            ILogger<AdjustmentRequestedEventHandler> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(AdjustmentRequested evt)
        {
            // Get approvers based on approval level
            var approvers = await _db.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive && 
                    (evt.ApprovalLevel == "Admin" && u.Role.RoleName == "Admin") ||
                    (evt.ApprovalLevel == "Manager" && (u.Role.RoleName == "Manager" || u.Role.RoleName == "Admin")))
                .ToListAsync();

            foreach (var approver in approvers)
            {
                var notification = new UserNotification
                {
                    NotificationId = System.Guid.NewGuid(),
                    RecipientUserId = approver.UserId,
                    RelatedEntityType = "Adjustment",
                    RelatedEntityId = evt.AdjustmentId,
                    EventType = "APPROVAL_REQUIRED",
                    Message = $"An adjustment request for quotation {evt.QuotationId} requires your approval.",
                    IsRead = false,
                    CreatedAt = System.DateTimeOffset.UtcNow
                };

                _db.Notifications.Add(notification);
                await _emailService.SendEmailNotificationAsync(notification, approver);
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("Adjustment requested event handled for adjustment {AdjustmentId}", evt.AdjustmentId);
        }
    }
}

