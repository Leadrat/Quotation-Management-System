using CRM.Domain.Entities;
using CRM.Application.Notifications.Services;

namespace CRM.Application.Notifications.Services
{
    public class QuotationAcceptedTemplate : BaseNotificationTemplate
    {
        public override string GetSubject(Notification notification)
        {
            return ReplacePlaceholders("Quotation {QuotationNumber} Accepted by {ClientName}", notification);
        }

        public override string GetBody(Notification notification)
        {
            return ReplacePlaceholders(@"
                <h2>Quotation Accepted!</h2>
                <p>Great news! {ClientName} has accepted quotation {QuotationNumber}.</p>
                <p><strong>Quotation Number:</strong> {QuotationNumber}</p>
                <p><strong>Client:</strong> {ClientName}</p>
                <p><strong>Total Amount:</strong> {TotalAmount}</p>
                <p>{Message}</p>
            ", notification);
        }
    }
}

