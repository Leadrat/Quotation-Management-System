using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("Jurisdictions")]
    public class Jurisdiction
    {
        public Guid JurisdictionId { get; set; }
        public Guid CountryId { get; set; }
        public Guid? ParentJurisdictionId { get; set; }
        public string JurisdictionName { get; set; } = string.Empty;
        public string? JurisdictionCode { get; set; }
        public string? JurisdictionType { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        // Navigation properties
        public virtual Country Country { get; set; } = null!;
        public virtual Jurisdiction? ParentJurisdiction { get; set; }
        public virtual ICollection<Jurisdiction> ChildJurisdictions { get; set; } = new List<Jurisdiction>();
        // public virtual ICollection<TaxRate> TaxRates { get; set; } = new List<TaxRate>(); // Commented out until TaxRate entity is created
        // public virtual ICollection<Client> Clients { get; set; } = new List<Client>(); // Commented out until Client entity is updated

        // Domain methods
        public bool IsDeleted() => DeletedAt != null;
    }
}

