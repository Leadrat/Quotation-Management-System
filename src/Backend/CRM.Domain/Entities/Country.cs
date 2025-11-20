using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities
{
    [Table("Countries")]
    public class Country
    {
        public Guid CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public TaxFrameworkType TaxFrameworkType { get; set; }
        public string DefaultCurrency { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        // Navigation properties (commented out until Jurisdiction entity is created)
        // public virtual ICollection<Jurisdiction> Jurisdictions { get; set; } = new List<Jurisdiction>();
        public virtual TaxFramework? TaxFramework { get; set; }
        // public virtual ICollection<Client> Clients { get; set; } = new List<Client>();

        // Domain methods
        public bool IsDeleted() => DeletedAt != null;
    }
}

