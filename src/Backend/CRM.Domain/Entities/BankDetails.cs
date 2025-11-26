using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    public class BankDetails
    {
        public Guid BankDetailsId { get; set; }
        public Guid CompanyDetailsId { get; set; }
        public string Country { get; set; } = string.Empty; // Keep for backward compatibility during transition
        public Guid? CountryId { get; set; } // New field for FK to Countries table (for country-specific functionality)
        public string AccountNumber { get; set; } = string.Empty;
        public string? IfscCode { get; set; }
        public string? Iban { get; set; }
        public string? SwiftCode { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string? BranchName { get; set; }
        
        [Column(TypeName = "jsonb")]
        public string? FieldValues { get; set; } // Country-specific bank field values as JSONB: { "IFSC": "...", "AccountNumber": "...", "BankName": "..." }
        
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Guid UpdatedBy { get; set; }

        // Navigation properties
        public virtual CompanyDetails? CompanyDetails { get; set; }
        public virtual Country? CountryNavigation { get; set; }
        public virtual User? UpdatedByUser { get; set; }
    }
}

