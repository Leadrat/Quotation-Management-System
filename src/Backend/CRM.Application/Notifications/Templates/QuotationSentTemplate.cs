using CRM.Domain.Entities;
using CRM.Application.Notifications.Services;

namespace CRM.Application.Notifications.Services
{
    public class QuotationSentTemplate : BaseNotificationTemplate
    {
        public override string GetSubject(UserNotification notification)
        {
            return ReplacePlaceholders("Quotation {QuotationNumber} Sent to {ClientName}", notification);
        }

        public override string GetBody(UserNotification notification)
        {
            return ReplacePlaceholders(@"
                <h2>Quotation Sent</h2>
                <p>Quotation {QuotationNumber} has been sent to {ClientName}.</p>
                <p><strong>Quotation Number:</strong> {QuotationNumber}</p>
                <p><strong>Client:</strong> {ClientName}</p>
                <p><strong>Total Amount:</strong> {TotalAmount}</p>
                <p>{Message}</p>
            ", notification);
        }
    }
}

