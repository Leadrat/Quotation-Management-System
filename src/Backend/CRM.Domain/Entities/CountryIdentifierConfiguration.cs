using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("CountryIdentifierConfigurations")]
    public class CountryIdentifierConfiguration
    {
        public Guid ConfigurationId { get; set; }
        public Guid CountryId { get; set; }
        public Guid IdentifierTypeId { get; set; }
        public bool IsRequired { get; set; } = false;
        public string? ValidationRegex { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? DisplayName { get; set; } // Override display name for this country
        public string? HelpText { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        // Navigation properties
        public virtual Country? Country { get; set; }
        public virtual IdentifierType? IdentifierType { get; set; }

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

