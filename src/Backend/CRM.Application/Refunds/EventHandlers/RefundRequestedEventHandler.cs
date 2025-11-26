using CRM.Domain.Entities;
using System.Threading.Tasks;
using CRM.Domain.Events;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Refunds.EventHandlers
{
    public class RefundRequestedEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly IEmailNotificationService _emailService;
        private readonly ILogger<RefundRequestedEventHandler> _logger;

        public RefundRequestedEventHandler(
            IAppDbContext db,
            IEmailNotificationService emailService,
            ILogger<RefundRequestedEventHandler> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(RefundRequested evt)
        {
            // Get approver based on approval level
            var approvers = await _db.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive && 
                    (evt.ApprovalLevel == "Admin" && u.Role.RoleName == "Admin") ||
                    (evt.ApprovalLevel == "Manager" && (u.Role.RoleName == "Manager" || u.Role.RoleName == "Admin")))
                .ToListAsync();

            // Create notification for each approver
            foreach (var approver in approvers)
            {
                var notification = new UserNotification
                {
                    NotificationId = System.Guid.NewGuid(),
                    RecipientUserId = approver.UserId,
                    RelatedEntityType = "Refund",
                    RelatedEntityId = evt.RefundId,
                    EventType = "APPROVAL_REQUIRED",
                    Message = $"A refund request of {evt.RefundAmount:C} has been submitted and requires your approval.",
                    IsRead = false,
                    CreatedAt = System.DateTimeOffset.UtcNow
                };

                _db.Notifications.Add(notification);

                // Send email notification
                await _emailService.SendEmailNotificationAsync(notification, approver);
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("Refund requested event handled for refund {RefundId}", evt.RefundId);
        }
    }
}

