using CRM.Domain.Entities;
using CRM.Application.Notifications.Services;

namespace CRM.Application.Notifications.Services
{
    public class ApprovalRejectedTemplate : BaseNotificationTemplate
    {
        public override string GetSubject(UserNotification notification)
        {
            return ReplacePlaceholders("Discount Approval Rejected for Quotation {QuotationNumber}", notification);
        }

        public override string GetBody(UserNotification notification)
        {
            return ReplacePlaceholders(@"
                <h2>Discount Approval Rejected</h2>
                <p>Your discount approval request for quotation {QuotationNumber} has been rejected.</p>
                <p><strong>Quotation Number:</strong> {QuotationNumber}</p>
                <p><strong>Client:</strong> {ClientName}</p>
                <p><strong>Discount Percentage:</strong> {DiscountPercentage}%</p>
                <p><strong>Reason:</strong> {Message}</p>
            ", notification);
        }
    }
}

