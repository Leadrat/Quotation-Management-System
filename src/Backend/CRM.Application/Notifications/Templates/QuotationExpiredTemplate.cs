using CRM.Domain.Entities;
using CRM.Application.Notifications.Services;

namespace CRM.Application.Notifications.Services
{
    public class QuotationExpiredTemplate : BaseNotificationTemplate
    {
        public override string GetSubject(Notification notification)
        {
            return ReplacePlaceholders("Quotation {QuotationNumber} Has Expired", notification);
        }

        public override string GetBody(Notification notification)
        {
            return ReplacePlaceholders(@"
                <h2>Quotation Expired</h2>
                <p>Quotation {QuotationNumber} has expired.</p>
                <p><strong>Quotation Number:</strong> {QuotationNumber}</p>
                <p><strong>Client:</strong> {ClientName}</p>
                <p><strong>Valid Until:</strong> {ValidUntil}</p>
                <p>{Message}</p>
            ", notification);
        }
    }
}

