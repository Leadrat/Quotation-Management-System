using System;
using System.Threading.Tasks;
using CRM.Application.Common.Notifications;
using CRM.Application.Common.Persistence;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.DiscountApprovals.EventHandlers
{
    public class DiscountApprovalApprovedEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly IEmailQueue _emailQueue;
        private readonly ILogger<DiscountApprovalApprovedEventHandler> _logger;

        public DiscountApprovalApprovedEventHandler(
            IAppDbContext db,
            IEmailQueue emailQueue,
            ILogger<DiscountApprovalApprovedEventHandler> logger)
        {
            _db = db;
            _emailQueue = emailQueue;
            _logger = logger;
        }

        public async Task Handle(DiscountApprovalApproved evt)
        {
            _logger.LogInformation("Handling DiscountApprovalApproved event for approval {ApprovalId}", evt.ApprovalId);

            // Create audit log entry
            _logger.LogInformation(
                "Discount approval approved: ApprovalId={ApprovalId}, QuotationId={QuotationId}, ApprovedBy={ApprovedByUserId}, Discount={DiscountPercentage}%",
                evt.ApprovalId, evt.QuotationId, evt.ApprovedByUserId, evt.DiscountPercentage);

            // Send email notification to sales rep
            var approval = await _db.DiscountApprovals
                .Include(a => a.RequestedByUser)
                .Include(a => a.Quotation)
                    .ThenInclude(q => q.Client)
                .FirstOrDefaultAsync(a => a.ApprovalId == evt.ApprovalId);

            if (approval?.RequestedByUser != null && approval.Quotation != null)
            {
                var subject = $"Discount Approved: {approval.Quotation.QuotationNumber}";
                var body = $@"
                    <h2>Discount Approval Approved</h2>
                    <p>Hello {approval.RequestedByUser.GetFullName()},</p>
                    <p>Your discount approval request for quotation <strong>{approval.Quotation.QuotationNumber}</strong> has been approved.</p>
                    <p><strong>Client:</strong> {approval.Quotation.Client.CompanyName}</p>
                    <p><strong>Approved Discount:</strong> {evt.DiscountPercentage}%</p>
                    <p><strong>Reason:</strong> {evt.Reason}</p>
                    {(string.IsNullOrEmpty(evt.Comments) ? "" : $"<p><strong>Comments:</strong> {evt.Comments}</p>")}
                    <p>The quotation has been updated with the approved discount.</p>
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

