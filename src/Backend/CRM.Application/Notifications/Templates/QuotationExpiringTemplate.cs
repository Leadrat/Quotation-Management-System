using CRM.Domain.Entities;
using CRM.Application.Notifications.Services;

namespace CRM.Application.Notifications.Services
{
    public class QuotationExpiringTemplate : BaseNotificationTemplate
    {
        public override string GetSubject(Notification notification)
        {
            return ReplacePlaceholders("Quotation {QuotationNumber} Expiring Soon", notification);
        }

        public override string GetBody(Notification notification)
        {
            return ReplacePlaceholders(@"
                <h2>Quotation Expiring Soon</h2>
                <p>Quotation {QuotationNumber} is expiring soon.</p>
                <p><strong>Quotation Number:</strong> {QuotationNumber}</p>
                <p><strong>Client:</strong> {ClientName}</p>
                <p><strong>Valid Until:</strong> {ValidUntil}</p>
                <p>{Message}</p>
            ", notification);
        }
    }
}

