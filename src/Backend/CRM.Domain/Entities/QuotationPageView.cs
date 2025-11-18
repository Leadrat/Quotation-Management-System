using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("QuotationPageViews")]
    public class QuotationPageView
    {
        public Guid ViewId { get; set; }
        public Guid AccessLinkId { get; set; }
        public Guid QuotationId { get; set; }
        public string ClientEmail { get; set; } = string.Empty;
        public DateTimeOffset ViewStartTime { get; set; }
        public DateTimeOffset? ViewEndTime { get; set; }
        public int? DurationSeconds { get; set; } // Calculated when view ends
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        // Navigation property
        public virtual QuotationAccessLink AccessLink { get; set; } = null!;
        public virtual Quotation Quotation { get; set; } = null!;

        public void EndView()
        {
            if (ViewEndTime == null)
            {
                ViewEndTime = DateTimeOffset.UtcNow;
                DurationSeconds = (int)(ViewEndTime.Value - ViewStartTime).TotalSeconds;
            }
        }
    }
}

