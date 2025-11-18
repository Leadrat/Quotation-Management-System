using System;
using System.Threading.Tasks;
using CRM.Application.Common.Notifications;
using CRM.Application.Common.Persistence;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.DiscountApprovals.EventHandlers
{
    public class DiscountApprovalRequestedEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly IEmailQueue _emailQueue;
        private readonly ILogger<DiscountApprovalRequestedEventHandler> _logger;

        public DiscountApprovalRequestedEventHandler(
            IAppDbContext db,
            IEmailQueue emailQueue,
            ILogger<DiscountApprovalRequestedEventHandler> logger)
        {
            _db = db;
            _emailQueue = emailQueue;
            _logger = logger;
        }

        public async Task Handle(DiscountApprovalRequested evt)
        {
            _logger.LogInformation("Handling DiscountApprovalRequested event for approval {ApprovalId}", evt.ApprovalId);

            // Create audit log entry (simplified - in production, use dedicated audit log service)
            _logger.LogInformation(
                "Discount approval requested: ApprovalId={ApprovalId}, QuotationId={QuotationId}, RequestedBy={RequestedByUserId}, Approver={ApproverUserId}, Discount={DiscountPercentage}%",
                evt.ApprovalId, evt.QuotationId, evt.RequestedByUserId, evt.ApproverUserId, evt.DiscountPercentage);

            // Send email notification to approver
            if (evt.ApproverUserId.HasValue)
            {
                var approver = await _db.Users
                    .FirstOrDefaultAsync(u => u.UserId == evt.ApproverUserId.Value);

                if (approver != null)
                {
                    var quotation = await _db.Quotations
                        .Include(q => q.Client)
                        .FirstOrDefaultAsync(q => q.QuotationId == evt.QuotationId);

                    if (quotation != null)
                    {
                        var subject = $"Discount Approval Request: {quotation.QuotationNumber}";
                        var body = $@"
                            <h2>Discount Approval Request</h2>
                            <p>Hello {approver.GetFullName()},</p>
                            <p>A discount approval request has been submitted for quotation <strong>{quotation.QuotationNumber}</strong>.</p>
                            <p><strong>Client:</strong> {quotation.Client.CompanyName}</p>
                            <p><strong>Discount Percentage:</strong> {evt.DiscountPercentage}%</p>
                            <p><strong>Approval Level:</strong> {evt.ApprovalLevel}</p>
                            <p><strong>Reason:</strong> {evt.Reason}</p>
                            {(string.IsNullOrEmpty(evt.Comments) ? "" : $"<p><strong>Comments:</strong> {evt.Comments}</p>")}
                            <p>Please review and approve or reject this request.</p>
                        ";

                        await _emailQueue.EnqueueAsync(new EmailMessage(
                            approver.Email,
                            subject,
                            body
                        ));
                    }
                }
            }
        }
    }
}

