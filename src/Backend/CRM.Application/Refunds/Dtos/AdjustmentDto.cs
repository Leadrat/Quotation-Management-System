using System;
using CRM.Domain.Enums;

namespace CRM.Application.Refunds.Dtos
{
    public class AdjustmentDto
    {
        public Guid AdjustmentId { get; set; }
        public Guid QuotationId { get; set; }
        public AdjustmentType AdjustmentType { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal AdjustedAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string RequestedByUserName { get; set; } = string.Empty;
        public string? ApprovedByUserName { get; set; }
        public AdjustmentStatus Status { get; set; }
        public string? ApprovalLevel { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public DateTimeOffset? ApprovalDate { get; set; }
        public DateTimeOffset? AppliedDate { get; set; }
        public decimal AdjustmentDifference { get; set; }
    }
}

