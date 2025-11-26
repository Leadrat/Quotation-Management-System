using CRM.Domain.Entities;
using CRM.Application.Notifications.Services;

namespace CRM.Application.Notifications.Services
{
    public class ApprovalNeededTemplate : BaseNotificationTemplate
    {
        public override string GetSubject(UserNotification notification)
        {
            return ReplacePlaceholders("Discount Approval Required for Quotation {QuotationNumber}", notification);
        }

        public override string GetBody(UserNotification notification)
        {
            return ReplacePlaceholders(@"
                <h2>Discount Approval Required</h2>
                <p>A discount approval request has been submitted for quotation {QuotationNumber}.</p>
                <p><strong>Quotation Number:</strong> {QuotationNumber}</p>
                <p><strong>Client:</strong> {ClientName}</p>
                <p><strong>Discount Percentage:</strong> {DiscountPercentage}%</p>
                <p><strong>Approval Level:</strong> {ApprovalLevel}</p>
                <p><strong>Reason:</strong> {Message}</p>
                <p>Please review and approve or reject this request.</p>
            ", notification);
        }
    }
}

