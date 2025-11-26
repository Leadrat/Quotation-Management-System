namespace CRM.Domain.Enums
{
    public enum NotificationEventType
    {
        QuotationCreated,
        QuotationSent,
        QuotationViewed,
        QuotationAccepted,
        QuotationRejected,
        QuotationUpdated,
        ApprovalNeeded,
        ApprovalApproved,
        ApprovalRejected,
        QuotationExpiring,
        QuotationExpired,
        ClientResponse,
        CommentMention,
        PaymentRequested,
        PaymentReceived,
        PaymentFailed,
        PaymentOverdue
    }
}

