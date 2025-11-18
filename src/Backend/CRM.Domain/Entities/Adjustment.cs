using System;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities
{
    /// <summary>
    /// Represents a billing adjustment to a quotation
    /// </summary>
    public class Adjustment
    {
        public Guid AdjustmentId { get; set; }
        public Guid QuotationId { get; set; }
        public AdjustmentType AdjustmentType { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal AdjustedAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public Guid RequestedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public AdjustmentStatus Status { get; set; } = AdjustmentStatus.PENDING;
        public string? ApprovalLevel { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public DateTimeOffset? ApprovalDate { get; set; }
        public DateTimeOffset? AppliedDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation properties
        public virtual Quotation Quotation { get; set; } = null!;
        public virtual User RequestedByUser { get; set; } = null!;
        public virtual User? ApprovedByUser { get; set; }

        // Domain methods
        public bool CanBeApproved() => Status == AdjustmentStatus.PENDING;
        
        public bool CanBeRejected() => Status == AdjustmentStatus.PENDING;
        
        public bool CanBeApplied() => Status == AdjustmentStatus.APPROVED;
        
        public decimal GetAdjustmentDifference() => AdjustedAmount - OriginalAmount;
        
        public void MarkAsApproved(Guid approverId, DateTimeOffset approvalDate)
        {
            if (!CanBeApproved())
                throw new InvalidOperationException("Adjustment cannot be approved in current status");
            
            Status = AdjustmentStatus.APPROVED;
            ApprovedByUserId = approverId;
            ApprovalDate = approvalDate;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        
        public void MarkAsRejected()
        {
            if (!CanBeRejected())
                throw new InvalidOperationException("Adjustment cannot be rejected in current status");
            
            Status = AdjustmentStatus.REJECTED;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        
        public void MarkAsApplied(DateTimeOffset appliedDate)
        {
            if (!CanBeApplied())
                throw new InvalidOperationException("Adjustment must be approved before applying");
            
            Status = AdjustmentStatus.APPLIED;
            AppliedDate = appliedDate;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}

