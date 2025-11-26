using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    public class CompanyDetails
    {
        public Guid CompanyDetailsId { get; set; } = new Guid("00000000-0000-0000-0000-000000000001");
        public string? PanNumber { get; set; }
        public string? TanNumber { get; set; }
        public string? GstNumber { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public Guid? CountryId { get; set; } // Foreign key to Countries table
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Website { get; set; }
        public string? LegalDisclaimer { get; set; }
        public string? LogoUrl { get; set; }
        
        [Column(TypeName = "jsonb")]
        public string? IdentifierValues { get; set; } // Country-specific identifier values as JSONB: { "countryId1": { "PAN": "...", "TAN": "..." }, "countryId2": { "VAT": "..." } }
        
        [Column(TypeName = "jsonb")]
        public string? CountryDetails { get; set; } // Country-specific basic company details as JSONB: { "countryId1": { "CompanyName": "...", "CompanyAddress": "...", "ContactEmail": "...", "LogoUrl": "..." }, "countryId2": { ... } }
        
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Guid UpdatedBy { get; set; }

        // Navigation properties
        public virtual User? UpdatedByUser { get; set; }
        public virtual ICollection<BankDetails> BankDetails { get; set; } = new List<BankDetails>();
        public virtual Country? CountryNavigation { get; set; } // Navigation property for CountryId
    }
}

