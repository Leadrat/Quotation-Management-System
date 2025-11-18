using System;
using System.Collections.Generic;

namespace CRM.Application.QuotationTemplates.Dtos
{
    public class QuotationTemplateDto
    {
        public Guid TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid OwnerUserId { get; set; }
        public string OwnerUserName { get; set; } = string.Empty;
        public string OwnerRole { get; set; } = string.Empty;
        public string Visibility { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
        public int Version { get; set; }
        public Guid? PreviousVersionId { get; set; }
        public int UsageCount { get; set; }
        public DateTimeOffset? LastUsedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public decimal? DiscountDefault { get; set; }
        public string? Notes { get; set; }
        public List<TemplateLineItemDto> LineItems { get; set; } = new();

        // Computed properties
        public bool IsActive => !DeletedAt.HasValue;
        public bool IsEditable => IsActive && (IsApproved || Visibility == "Private");
    }
}

