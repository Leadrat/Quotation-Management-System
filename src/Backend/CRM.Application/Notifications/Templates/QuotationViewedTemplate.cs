using CRM.Domain.Entities;
using CRM.Application.Notifications.Services;

namespace CRM.Application.Notifications.Services
{
    public class QuotationViewedTemplate : BaseNotificationTemplate
    {
        public override string GetSubject(UserNotification notification)
        {
            return ReplacePlaceholders("Client Viewed Quotation {QuotationNumber}", notification);
        }

        public override string GetBody(UserNotification notification)
        {
            return ReplacePlaceholders(@"
                <h2>Quotation Viewed</h2>
                <p>Your client has viewed quotation {QuotationNumber}.</p>
                <p><strong>Quotation Number:</strong> {QuotationNumber}</p>
                <p><strong>Client:</strong> {ClientName}</p>
                <p>{Message}</p>
            ", notification);
        }
    }
}

