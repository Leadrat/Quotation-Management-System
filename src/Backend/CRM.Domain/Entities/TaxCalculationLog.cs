using System;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities
{
    [Table("TaxCalculationLogs")]
    public class TaxCalculationLog
    {
        public Guid LogId { get; set; }
        public Guid? QuotationId { get; set; }
        public TaxCalculationActionType ActionType { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? JurisdictionId { get; set; }
        public string CalculationDetails { get; set; } = string.Empty; // JSONB
        public Guid ChangedByUserId { get; set; }
        public DateTimeOffset ChangedAt { get; set; }

        // Navigation properties
        public virtual Quotation? Quotation { get; set; }
        public virtual Country? Country { get; set; }
        public virtual Jurisdiction? Jurisdiction { get; set; }
        public virtual User ChangedByUser { get; set; } = null!;
    }
}

