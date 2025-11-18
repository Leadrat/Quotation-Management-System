using System;
using System.Threading.Tasks;
using CRM.Application.Common.Notifications;
using CRM.Application.Common.Persistence;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.DiscountApprovals.EventHandlers
{
    public class DiscountApprovalEscalatedEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly IEmailQueue _emailQueue;
        private readonly ILogger<DiscountApprovalEscalatedEventHandler> _logger;

        public DiscountApprovalEscalatedEventHandler(
            IAppDbContext db,
            IEmailQueue emailQueue,
            ILogger<DiscountApprovalEscalatedEventHandler> logger)
        {
            _db = db;
            _emailQueue = emailQueue;
            _logger = logger;
        }

        public async Task Handle(DiscountApprovalEscalated evt)
        {
            _logger.LogInformation("Handling DiscountApprovalEscalated event for approval {ApprovalId}", evt.ApprovalId);

            // Create audit log entry
            _logger.LogInformation(
                "Discount approval escalated: ApprovalId={ApprovalId}, QuotationId={QuotationId}, EscalatedBy={EscalatedByUserId}, Admin={AdminUserId}",
                evt.ApprovalId, evt.QuotationId, evt.EscalatedByUserId, evt.AdminUserId);

            // Send email notification to admin
            var admin = await _db.Users
                .FirstOrDefaultAsync(u => u.UserId == evt.AdminUserId);

            if (admin != null)
            {
                var approval = await _db.DiscountApprovals
                    .Include(a => a.Quotation)
                        .ThenInclude(q => q.Client)
                    .FirstOrDefaultAsync(a => a.ApprovalId == evt.ApprovalId);

                if (approval?.Quotation != null)
                {
                    var subject = $"Discount Approval Escalated: {approval.Quotation.QuotationNumber}";
                    var body = $@"
                        <h2>Discount Approval Escalated</h2>
                        <p>Hello {admin.GetFullName()},</p>
                        <p>A discount approval request for quotation <strong>{approval.Quotation.QuotationNumber}</strong> has been escalated to you.</p>
                        <p><strong>Client:</strong> {approval.Quotation.Client.CompanyName}</p>
                        <p><strong>Discount Percentage:</strong> {approval.CurrentDiscountPercentage}%</p>
                        {(string.IsNullOrEmpty(evt.Reason) ? "" : $"<p><strong>Escalation Reason:</strong> {evt.Reason}</p>")}
                        <p>Please review and approve or reject this request.</p>
                    ";

                    await _emailQueue.EnqueueAsync(new EmailMessage(
                        admin.Email,
                        subject,
                        body
                    ));
                }
            }
        }
    }
}

