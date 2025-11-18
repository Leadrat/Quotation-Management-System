using System;
using System.Threading.Tasks;
using CRM.Application.Common.Notifications;
using CRM.Application.Common.Persistence;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.DiscountApprovals.EventHandlers
{
    public class DiscountApprovalResubmittedEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly IEmailQueue _emailQueue;
        private readonly ILogger<DiscountApprovalResubmittedEventHandler> _logger;

        public DiscountApprovalResubmittedEventHandler(
            IAppDbContext db,
            IEmailQueue emailQueue,
            ILogger<DiscountApprovalResubmittedEventHandler> logger)
        {
            _db = db;
            _emailQueue = emailQueue;
            _logger = logger;
        }

        public async Task Handle(DiscountApprovalResubmitted evt)
        {
            _logger.LogInformation("Handling DiscountApprovalResubmitted event for approval {NewApprovalId}", evt.NewApprovalId);

            // Create audit log entry
            _logger.LogInformation(
                "Discount approval resubmitted: NewApprovalId={NewApprovalId}, PreviousApprovalId={PreviousApprovalId}, QuotationId={QuotationId}, ResubmittedBy={ResubmittedByUserId}",
                evt.NewApprovalId, evt.PreviousApprovalId, evt.QuotationId, evt.ResubmittedByUserId);

            // Send email notification to approver
            var newApproval = await _db.DiscountApprovals
                .Include(a => a.ApproverUser)
                .Include(a => a.Quotation)
                    .ThenInclude(q => q.Client)
                .FirstOrDefaultAsync(a => a.ApprovalId == evt.NewApprovalId);

            if (newApproval?.ApproverUser != null && newApproval.Quotation != null)
            {
                var subject = $"Discount Approval Resubmitted: {newApproval.Quotation.QuotationNumber}";
                var body = $@"
                    <h2>Discount Approval Resubmitted</h2>
                    <p>Hello {newApproval.ApproverUser.GetFullName()},</p>
                    <p>A discount approval request for quotation <strong>{newApproval.Quotation.QuotationNumber}</strong> has been resubmitted.</p>
                    <p><strong>Client:</strong> {newApproval.Quotation.Client.CompanyName}</p>
                    <p><strong>Discount Percentage:</strong> {newApproval.CurrentDiscountPercentage}%</p>
                    <p><strong>Reason:</strong> {evt.Reason}</p>
                    {(string.IsNullOrEmpty(evt.Comments) ? "" : $"<p><strong>Comments:</strong> {evt.Comments}</p>")}
                    <p>Please review and approve or reject this request.</p>
                ";

                await _emailQueue.EnqueueAsync(new EmailMessage(
                    newApproval.ApproverUser.Email,
                    subject,
                    body
                ));
            }
        }
    }
}

