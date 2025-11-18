using CRM.Domain.Entities;
using CRM.Application.Notifications.Services;

namespace CRM.Application.Notifications.Services
{
    public class ClientResponseTemplate : BaseNotificationTemplate
    {
        public override string GetSubject(Notification notification)
        {
            return ReplacePlaceholders("Client Response Received for Quotation {QuotationNumber}", notification);
        }

        public override string GetBody(Notification notification)
        {
            return ReplacePlaceholders(@"
                <h2>Client Response Received</h2>
                <p>A client response has been received for quotation {QuotationNumber}.</p>
                <p><strong>Quotation Number:</strong> {QuotationNumber}</p>
                <p><strong>Client:</strong> {ClientName}</p>
                <p><strong>Response Type:</strong> {ResponseType}</p>
                <p>{Message}</p>
            ", notification);
        }
    }
}

