using CRM.Domain.Entities;
using CRM.Application.Notifications.Services;

namespace CRM.Application.Notifications.Services
{
    public class ApprovalApprovedTemplate : BaseNotificationTemplate
    {
        public override string GetSubject(Notification notification)
        {
            return ReplacePlaceholders("Discount Approval Approved for Quotation {QuotationNumber}", notification);
        }

        public override string GetBody(Notification notification)
        {
            return ReplacePlaceholders(@"
                <h2>Discount Approval Approved</h2>
                <p>Your discount approval request for quotation {QuotationNumber} has been approved.</p>
                <p><strong>Quotation Number:</strong> {QuotationNumber}</p>
                <p><strong>Client:</strong> {ClientName}</p>
                <p><strong>Discount Percentage:</strong> {DiscountPercentage}%</p>
                <p>{Message}</p>
            ", notification);
        }
    }
}

