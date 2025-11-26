using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("BankFieldTypes")]
    public class BankFieldType
    {
        public Guid BankFieldTypeId { get; set; }
        public string Name { get; set; } = string.Empty; // IFSC, IBAN, SWIFT, ROUTING_NUMBER
        public string DisplayName { get; set; } = string.Empty; // IFSC Code, IBAN, SWIFT Code
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        // Navigation properties
        public virtual ICollection<CountryBankFieldConfiguration> CountryBankFieldConfigurations { get; set; } = new List<CountryBankFieldConfiguration>();

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

