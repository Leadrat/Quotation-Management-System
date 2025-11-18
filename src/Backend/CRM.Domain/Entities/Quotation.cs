using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities
{
    [Table("Quotations")]
    public class Quotation
    {
        public Guid QuotationId { get; set; }
        public Guid ClientId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public QuotationStatus Status { get; set; } = QuotationStatus.Draft;
        public DateTime QuotationDate { get; set; }
        public DateTime ValidUntil { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal? CgstAmount { get; set; }
        public decimal? SgstAmount { get; set; }
        public decimal? IgstAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public bool IsPendingApproval { get; set; }
        public Guid? PendingApprovalId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation properties
        public virtual Client Client { get; set; } = null!;
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual ICollection<QuotationLineItem> LineItems { get; set; } = new List<QuotationLineItem>();
        public virtual DiscountApproval? PendingApproval { get; set; }

        // Domain methods
        public bool IsExpired() => ValidUntil < DateTime.Today;
        
        public bool CanBeEdited() => Status == QuotationStatus.Draft && !IsPendingApproval;
        
        public bool IsLockedForEditing() => IsPendingApproval;
        
        public void LockForApproval(Guid approvalId)
        {
            IsPendingApproval = true;
            PendingApprovalId = approvalId;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        
        public void UnlockFromApproval()
        {
            IsPendingApproval = false;
            PendingApprovalId = null;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        
        public void MarkAsViewed()
        {
            if (Status == QuotationStatus.Sent)
            {
                Status = QuotationStatus.Viewed;
                UpdatedAt = DateTimeOffset.UtcNow;
            }
        }
        
        public void MarkAsAccepted()
        {
            if (Status == QuotationStatus.Viewed || Status == QuotationStatus.Sent)
            {
                Status = QuotationStatus.Accepted;
                UpdatedAt = DateTimeOffset.UtcNow;
            }
        }
        
        public void MarkAsRejected()
        {
            if (Status == QuotationStatus.Viewed || Status == QuotationStatus.Sent)
            {
                Status = QuotationStatus.Rejected;
                UpdatedAt = DateTimeOffset.UtcNow;
            }
        }
    }
}

