using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities
{
    [Table("QuotationTemplates")]
    public class QuotationTemplate
    {
        public Guid TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid OwnerUserId { get; set; }
        public string OwnerRole { get; set; } = "SalesRep";
        public TemplateVisibility Visibility { get; set; }
        public bool IsApproved { get; set; } = false;
        public Guid? ApprovedByUserId { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
        public int Version { get; set; } = 1;
        public Guid? PreviousVersionId { get; set; }
        public int UsageCount { get; set; } = 0;
        public DateTimeOffset? LastUsedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        // Additional fields for template defaults
        public decimal? DiscountDefault { get; set; }
        public string? Notes { get; set; }

        // File-based template properties
        public string? TemplateType { get; set; } // "Quotation" or "ProFormaInvoice"
        public bool IsFileBased { get; set; } = false;
        public string? FileName { get; set; }
        public string? FileUrl { get; set; } // Path or URL to stored file
        public long? FileSize { get; set; } // File size in bytes
        public string? MimeType { get; set; } // MIME type of the file

        // Navigation properties
        public virtual User OwnerUser { get; set; } = null!;
        public virtual User? ApprovedByUser { get; set; }
        public virtual QuotationTemplate? PreviousVersion { get; set; }
        public virtual ICollection<QuotationTemplateLineItem> LineItems { get; set; } = new List<QuotationTemplateLineItem>();

        // Domain methods
        public bool IsDeleted() => DeletedAt.HasValue;

        public bool CanBeEdited()
        {
            return !IsDeleted() && (IsApproved || Visibility == TemplateVisibility.Private);
        }

        public void IncrementVersion()
        {
            Version++;
        }

        public void MarkAsApproved(Guid approvedByUserId)
        {
            IsApproved = true;
            ApprovedByUserId = approvedByUserId;
            ApprovedAt = DateTimeOffset.UtcNow;
        }
    }
}

