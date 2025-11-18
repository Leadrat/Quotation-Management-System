using System;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities
{
    [Table("Payments")]
    public class Payment
    {
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public string PaymentGateway { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public string Currency { get; set; } = "INR";
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public DateTimeOffset? PaymentDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string? FailureReason { get; set; }
        public bool IsRefundable { get; set; } = true;
        public decimal? RefundAmount { get; set; }
        public string? RefundReason { get; set; }
        public DateTimeOffset? RefundDate { get; set; }
        public string? Metadata { get; set; } // JSON string for additional data

        // Navigation properties
        public virtual Quotation Quotation { get; set; } = null!;

        // Domain methods
        public void MarkAsSuccess(DateTimeOffset paymentDate)
        {
            PaymentStatus = PaymentStatus.Success;
            PaymentDate = paymentDate;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void MarkAsProcessing()
        {
            PaymentStatus = PaymentStatus.Processing;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void MarkAsFailed(string reason)
        {
            PaymentStatus = PaymentStatus.Failed;
            FailureReason = reason;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void ProcessRefund(decimal amount, string reason)
        {
            if (!IsRefundable)
                throw new InvalidOperationException("Payment is not refundable");

            if (amount <= 0)
                throw new InvalidOperationException("Refund amount must be greater than zero");

            if (amount > AmountPaid)
                throw new InvalidOperationException("Refund amount cannot exceed payment amount");

            if (RefundAmount.HasValue && RefundAmount.Value + amount > AmountPaid)
                throw new InvalidOperationException("Total refund amount cannot exceed payment amount");

            RefundAmount = (RefundAmount ?? 0) + amount;
            RefundReason = reason;
            RefundDate = DateTimeOffset.UtcNow;

            if (RefundAmount >= AmountPaid)
                PaymentStatus = PaymentStatus.Refunded;
            else
                PaymentStatus = PaymentStatus.PartiallyRefunded;

            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Cancel()
        {
            if (PaymentStatus != PaymentStatus.Pending && PaymentStatus != PaymentStatus.Processing)
                throw new InvalidOperationException("Only pending or processing payments can be cancelled");

            PaymentStatus = PaymentStatus.Cancelled;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public bool CanBeRefunded()
        {
            return IsRefundable && 
                   PaymentStatus == PaymentStatus.Success && 
                   (RefundAmount == null || RefundAmount < AmountPaid);
        }

        public bool CanBeCancelled()
        {
            return PaymentStatus == PaymentStatus.Pending || PaymentStatus == PaymentStatus.Processing;
        }
    }
}

