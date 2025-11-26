using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using CRM.Domain.Enums;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.EventHandlers
{
    public class ApprovalRequestHandler
    {
        private readonly IAppDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ApprovalRequestHandler> _logger;

        public ApprovalRequestHandler(
            IAppDbContext db,
            INotificationService notificationService,
            ILogger<ApprovalRequestHandler> logger)
        {
            _db = db;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task HandleDiscountApprovalRequested(DiscountApprovalRequested evt)
        {
            try
            {
                var approval = await _db.DiscountApprovals
                    .Include(a => a.Quotation)
                        .ThenInclude(q => q.Client)
                    .Include(a => a.RequestedByUser)
                    .FirstOrDefaultAsync(a => a.ApprovalId == evt.ApprovalId);

                if (approval == null)
                {
                    _logger.LogWarning("Discount approval {ApprovalId} not found for notification", evt.ApprovalId);
                    return;
                }

                var meta = new Dictionary<string, object>
                {
                    { "ApprovalId", evt.ApprovalId.ToString() },
                    { "QuotationNumber", approval.Quotation?.QuotationNumber ?? "Unknown" },
                    { "ClientName", approval.Quotation?.Client?.CompanyName ?? "Unknown" },
                    { "DiscountPercentage", evt.DiscountPercentage.ToString("F2") },
                    { "ApprovalLevel", evt.ApprovalLevel },
                    { "Reason", evt.Reason },
                    { "RequestedBy", approval.RequestedByUser?.FirstName + " " + approval.RequestedByUser?.LastName }
                };

                // Notify the approver if specified
                if (evt.ApproverUserId.HasValue)
                {
                    await _notificationService.PublishNotificationAsync(
                        NotificationEventType.ApprovalNeeded,
                        "DiscountApproval",
                        evt.ApprovalId,
                        evt.ApproverUserId.Value,
                        $"Discount approval required: {evt.DiscountPercentage:F2}% for quotation {approval.Quotation?.QuotationNumber}",
                        meta,
                        new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                    );
                }

                _logger.LogInformation("Notification published for DiscountApprovalRequested event: {ApprovalId}", evt.ApprovalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle DiscountApprovalRequested event for {ApprovalId}", evt.ApprovalId);
            }
        }

        public async Task HandleDiscountApprovalApproved(DiscountApprovalApproved evt)
        {
            try
            {
                var approval = await _db.DiscountApprovals
                    .Include(a => a.Quotation)
                        .ThenInclude(q => q.Client)
                    .Include(a => a.RequestedByUser)
                    .Include(a => a.ApproverUser)
                    .FirstOrDefaultAsync(a => a.ApprovalId == evt.ApprovalId);

                if (approval == null)
                {
                    _logger.LogWarning("Discount approval {ApprovalId} not found for notification", evt.ApprovalId);
                    return;
                }

                var meta = new Dictionary<string, object>
                {
                    { "ApprovalId", evt.ApprovalId.ToString() },
                    { "QuotationNumber", approval.Quotation?.QuotationNumber ?? "Unknown" },
                    { "ClientName", approval.Quotation?.Client?.CompanyName ?? "Unknown" },
                    { "ApprovedBy", approval.ApproverUser?.FirstName + " " + approval.ApproverUser?.LastName },
                    { "ApprovedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") }
                };

                // Notify the requester
                await _notificationService.PublishNotificationAsync(
                    NotificationEventType.ApprovalApproved,
                    "DiscountApproval",
                    evt.ApprovalId,
                    approval.RequestedByUserId,
                    $"Discount approval approved for quotation {approval.Quotation?.QuotationNumber}",
                    meta,
                    new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                );

                _logger.LogInformation("Notification published for DiscountApprovalApproved event: {ApprovalId}", evt.ApprovalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle DiscountApprovalApproved event for {ApprovalId}", evt.ApprovalId);
            }
        }

        public async Task HandleDiscountApprovalRejected(DiscountApprovalRejected evt)
        {
            try
            {
                var approval = await _db.DiscountApprovals
                    .Include(a => a.Quotation)
                        .ThenInclude(q => q.Client)
                    .Include(a => a.RequestedByUser)
                    .Include(a => a.ApproverUser)
                    .FirstOrDefaultAsync(a => a.ApprovalId == evt.ApprovalId);

                if (approval == null)
                {
                    _logger.LogWarning("Discount approval {ApprovalId} not found for notification", evt.ApprovalId);
                    return;
                }

                var meta = new Dictionary<string, object>
                {
                    { "ApprovalId", evt.ApprovalId.ToString() },
                    { "QuotationNumber", approval.Quotation?.QuotationNumber ?? "Unknown" },
                    { "ClientName", approval.Quotation?.Client?.CompanyName ?? "Unknown" },
                    { "RejectedBy", approval.ApproverUser?.FirstName + " " + approval.ApproverUser?.LastName },
                    { "RejectionReason", evt.RejectionReason ?? "No reason provided" },
                    { "RejectedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") }
                };

                // Notify the requester
                await _notificationService.PublishNotificationAsync(
                    NotificationEventType.ApprovalRejected,
                    "DiscountApproval",
                    evt.ApprovalId,
                    approval.RequestedByUserId,
                    $"Discount approval rejected for quotation {approval.Quotation?.QuotationNumber}",
                    meta,
                    new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                );

                _logger.LogInformation("Notification published for DiscountApprovalRejected event: {ApprovalId}", evt.ApprovalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle DiscountApprovalRejected event for {ApprovalId}", evt.ApprovalId);
            }
        }

        public async Task HandleDiscountApprovalEscalated(DiscountApprovalEscalated evt)
        {
            try
            {
                var approval = await _db.DiscountApprovals
                    .Include(a => a.Quotation)
                        .ThenInclude(q => q.Client)
                    .Include(a => a.RequestedByUser)
                    .FirstOrDefaultAsync(a => a.ApprovalId == evt.ApprovalId);

                if (approval == null)
                {
                    _logger.LogWarning("Discount approval {ApprovalId} not found for notification", evt.ApprovalId);
                    return;
                }

                var meta = new Dictionary<string, object>
                {
                    { "ApprovalId", evt.ApprovalId.ToString() },
                    { "QuotationNumber", approval.Quotation?.QuotationNumber ?? "Unknown" },
                    { "ClientName", approval.Quotation?.Client?.CompanyName ?? "Unknown" },
                    { "EscalatedTo", evt.EscalatedToLevel },
                    { "EscalatedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") }
                };

                // Notify the new approver if specified
                if (evt.NewApproverUserId != Guid.Empty)
                {
                    await _notificationService.PublishNotificationAsync(
                        NotificationEventType.ApprovalNeeded,
                        "DiscountApproval",
                        evt.ApprovalId,
                        evt.NewApproverUserId,
                        $"Escalated discount approval required for quotation {approval.Quotation?.QuotationNumber}",
                        meta,
                        new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                    );
                }

                _logger.LogInformation("Notification published for DiscountApprovalEscalated event: {ApprovalId}", evt.ApprovalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle DiscountApprovalEscalated event for {ApprovalId}", evt.ApprovalId);
            }
        }
    }
}