using CRM.Domain.Enums;

namespace CRM.Application.Notifications.Services
{
    public class NotificationTemplateService : INotificationTemplateService
    {
        public INotificationTemplate GetTemplate(NotificationEventType eventType)
        {
            return eventType switch
            {
                NotificationEventType.QuotationCreated => new QuotationCreatedTemplate(),
                NotificationEventType.QuotationSent => new QuotationSentTemplate(),
                NotificationEventType.QuotationViewed => new QuotationViewedTemplate(),
                NotificationEventType.QuotationAccepted => new QuotationAcceptedTemplate(),
                NotificationEventType.QuotationRejected => new QuotationRejectedTemplate(),
                NotificationEventType.ApprovalNeeded => new ApprovalNeededTemplate(),
                NotificationEventType.ApprovalApproved => new ApprovalApprovedTemplate(),
                NotificationEventType.ApprovalRejected => new ApprovalRejectedTemplate(),
                NotificationEventType.QuotationExpiring => new QuotationExpiringTemplate(),
                NotificationEventType.QuotationExpired => new QuotationExpiredTemplate(),
                NotificationEventType.ClientResponse => new ClientResponseTemplate(),
                _ => new DefaultNotificationTemplate()
            };
        }
    }
}

