using System;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities
{
    /// <summary>
    /// Represents a refund request and its processing status
    /// </summary>
    public class Refund
    {
        public Guid RefundId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid QuotationId { get; set; }
        public decimal RefundAmount { get; set; }
        public string RefundReason { get; set; } = string.Empty;
        public RefundReasonCode RefundReasonCode { get; set; }
        public Guid RequestedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public RefundStatus RefundStatus { get; set; } = RefundStatus.Pending;
        public string? PaymentGatewayReference { get; set; }
        public string? ApprovalLevel { get; set; }
        public string? Comments { get; set; }
        public string? FailureReason { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public DateTimeOffset? ApprovalDate { get; set; }
        public DateTimeOffset? CompletedDate { get; set; }
        public DateTimeOffset? ReversedDate { get; set; }
        public string? ReversedReason { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation properties
        public virtual Payment Payment { get; set; } = null!;
        public virtual Quotation Quotation { get; set; } = null!;
        public virtual User RequestedByUser { get; set; } = null!;
        public virtual User? ApprovedByUser { get; set; }
        public virtual ICollection<RefundTimeline> Timeline { get; set; } = new List<RefundTimeline>();

        // Domain methods
        public bool CanBeApproved() => RefundStatus == RefundStatus.Pending;
        
        public bool CanBeRejected() => RefundStatus == RefundStatus.Pending;
        
        public bool CanBeReversed() => RefundStatus == RefundStatus.Completed;
        
        public bool IsRefundable() => RefundStatus == RefundStatus.Pending || RefundStatus == RefundStatus.Approved;
        
        public void MarkAsApproved(Guid approverId, DateTimeOffset approvalDate)
        {
            if (!CanBeApproved())
                throw new InvalidOperationException("Refund cannot be approved in current status");
            
            RefundStatus = RefundStatus.Approved;
            ApprovedByUserId = approverId;
            ApprovalDate = approvalDate;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        
        public void MarkAsRejected(string reason)
        {
            if (!CanBeRejected())
                throw new InvalidOperationException("Refund cannot be rejected in current status");
            
            RefundStatus = RefundStatus.Failed; // Using Failed for rejected
            FailureReason = reason;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        
        public void MarkAsProcessing()
        {
            if (RefundStatus != RefundStatus.Approved)
                throw new InvalidOperationException("Refund must be approved before processing");
            
            RefundStatus = RefundStatus.Processing;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        
        public void MarkAsCompleted(string gatewayReference, DateTimeOffset completedDate)
        {
            if (RefundStatus != RefundStatus.Processing)
                throw new InvalidOperationException("Refund must be processing before completion");
            
            RefundStatus = RefundStatus.Completed;
            PaymentGatewayReference = gatewayReference;
            CompletedDate = completedDate;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        
        public void MarkAsFailed(string failureReason)
        {
            RefundStatus = RefundStatus.Failed;
            FailureReason = failureReason;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        
        public void MarkAsReversed(string reversedReason, DateTimeOffset reversedDate)
        {
            if (!CanBeReversed())
                throw new InvalidOperationException("Refund cannot be reversed in current status");
            
            RefundStatus = RefundStatus.Reversed;
            ReversedReason = reversedReason;
            ReversedDate = reversedDate;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}

