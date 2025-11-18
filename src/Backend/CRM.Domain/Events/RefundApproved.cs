using System;

namespace CRM.Domain.Events
{
    public class RefundApproved
    {
        public Guid RefundId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public decimal RefundAmount { get; set; }
        public Guid RequestedByUserId { get; set; }
        public Guid ApprovedByUserId { get; set; }
        public DateTimeOffset ApprovalDate { get; set; }
    }
}

