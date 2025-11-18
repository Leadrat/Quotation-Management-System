using System;
using System.Threading.Tasks;
using CRM.Application.Common.Notifications;
using CRM.Application.Common.Persistence;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.DiscountApprovals.EventHandlers
{
    public class DiscountApprovalRejectedEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly IEmailQueue _emailQueue;
        private readonly ILogger<DiscountApprovalRejectedEventHandler> _logger;

        public DiscountApprovalRejectedEventHandler(
            IAppDbContext db,
            IEmailQueue emailQueue,
            ILogger<DiscountApprovalRejectedEventHandler> logger)
        {
            _db = db;
            _emailQueue = emailQueue;
            _logger = logger;
        }

        public async Task Handle(DiscountApprovalRejected evt)
        {
            _logger.LogInformation("Handling DiscountApprovalRejected event for approval {ApprovalId}", evt.ApprovalId);

            // Create audit log entry
            _logger.LogInformation(
                "Discount approval rejected: ApprovalId={ApprovalId}, QuotationId={QuotationId}, RejectedBy={RejectedByUserId}",
                evt.ApprovalId, evt.QuotationId, evt.RejectedByUserId);

            // Send email notification to sales rep
            var approval = await _db.DiscountApprovals
                .Include(a => a.RequestedByUser)
                .Include(a => a.Quotation)
                    .ThenInclude(q => q.Client)
                .FirstOrDefaultAsync(a => a.ApprovalId == evt.ApprovalId);

            if (approval?.RequestedByUser != null && approval.Quotation != null)
            {
                var subject = $"Discount Rejected: {approval.Quotation.QuotationNumber}";
                var body = $@"
                    <h2>Discount Approval Rejected</h2>
                    <p>Hello {approval.RequestedByUser.GetFullName()},</p>
                    <p>Your discount approval request for quotation <strong>{approval.Quotation.QuotationNumber}</strong> has been rejected.</p>
                    <p><strong>Client:</strong> {approval.Quotation.Client.CompanyName}</p>
                    <p><strong>Reason:</strong> {evt.Reason}</p>
                    {(string.IsNullOrEmpty(evt.Comments) ? "" : $"<p><strong>Comments:</strong> {evt.Comments}</p>")}
                    <p>You can resubmit the approval request with a different discount or reason.</p>
                ";

                await _emailQueue.EnqueueAsync(new EmailMessage(
                    approval.RequestedByUser.Email,
                    subject,
                    body
                ));
            }
        }
    }
}

