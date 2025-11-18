using System;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities
{
    [Table("DiscountApprovals")]
    public class DiscountApproval
    {
        public Guid ApprovalId { get; set; }
        public Guid QuotationId { get; set; }
        public Guid RequestedByUserId { get; set; }
        public Guid? ApproverUserId { get; set; }
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
        public DateTimeOffset RequestDate { get; set; }
        public DateTimeOffset? ApprovalDate { get; set; }
        public DateTimeOffset? RejectionDate { get; set; }
        public decimal CurrentDiscountPercentage { get; set; }
        public decimal Threshold { get; set; }
        public ApprovalLevel ApprovalLevel { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public bool EscalatedToAdmin { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation properties
        public virtual Quotation Quotation { get; set; } = null!;
        public virtual User RequestedByUser { get; set; } = null!;
        public virtual User? ApproverUser { get; set; }

        // Domain methods
        public bool IsPending() => Status == ApprovalStatus.Pending;
        public bool IsApproved() => Status == ApprovalStatus.Approved;
        public bool IsRejected() => Status == ApprovalStatus.Rejected;

        public void Approve(Guid approverUserId, string reason, string? comments = null)
        {
            if (!IsPending())
                throw new InvalidOperationException("Only pending approvals can be approved.");

            Status = ApprovalStatus.Approved;
            ApproverUserId = approverUserId;
            ApprovalDate = DateTimeOffset.UtcNow;
            Reason = reason;
            Comments = comments;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Reject(Guid rejectorUserId, string reason, string? comments = null)
        {
            if (!IsPending())
                throw new InvalidOperationException("Only pending approvals can be rejected.");

            Status = ApprovalStatus.Rejected;
            ApproverUserId = rejectorUserId;
            RejectionDate = DateTimeOffset.UtcNow;
            Reason = reason;
            Comments = comments;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Escalate(Guid escalatedByUserId, Guid adminUserId, string? reason = null)
        {
            if (!IsPending())
                throw new InvalidOperationException("Only pending approvals can be escalated.");

            EscalatedToAdmin = true;
            ApprovalLevel = ApprovalLevel.Admin;
            ApproverUserId = adminUserId;
            if (!string.IsNullOrEmpty(reason))
                Comments = $"{Comments}\n[Escalated]: {reason}".Trim();
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public bool CanBeApprovedBy(Guid userId, string userRole)
        {
            if (IsApproved() || IsRejected())
                return false;

            // Admin can approve any approval
            if (userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return true;

            // Manager can approve manager-level approvals
            if (userRole.Equals("Manager", StringComparison.OrdinalIgnoreCase) && ApprovalLevel == ApprovalLevel.Manager)
                return ApproverUserId == userId || ApproverUserId == null;

            // User can only approve if they are the assigned approver
            return ApproverUserId == userId;
        }
    }
}

