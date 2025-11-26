using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("IdentifierTypes")]
    public class IdentifierType
    {
        public Guid IdentifierTypeId { get; set; }
        public string Name { get; set; } = string.Empty; // PAN, VAT, BUSINESS_LICENSE
        public string DisplayName { get; set; } = string.Empty; // PAN Number, VAT Number
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        // Navigation properties
        public virtual ICollection<CountryIdentifierConfiguration> CountryIdentifierConfigurations { get; set; } = new List<CountryIdentifierConfiguration>();

        // Domain methods
        public bool IsDeleted() => DeletedAt != null;
        public void SoftDelete()
        {
            DeletedAt = DateTimeOffset.UtcNow;
            IsActive = false;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}

