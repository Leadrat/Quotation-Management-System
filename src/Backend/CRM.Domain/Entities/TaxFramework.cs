using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Domain.Enums;
using System.Text.Json;

namespace CRM.Domain.Entities
{
    [Table("TaxFrameworks")]
    public class TaxFramework
    {
        public Guid TaxFrameworkId { get; set; }
        public Guid CountryId { get; set; }
        public string FrameworkName { get; set; } = string.Empty;
        public TaxFrameworkType FrameworkType { get; set; }
        public string? Description { get; set; }
        public string TaxComponents { get; set; } = string.Empty; // JSONB
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        // Navigation properties
        public virtual Country Country { get; set; } = null!;
        // public virtual ICollection<TaxRate> TaxRates { get; set; } = new List<TaxRate>(); // Commented out until TaxRate entity is created

        // Domain methods
        public bool IsDeleted() => DeletedAt != null;

        public List<TaxComponent> GetTaxComponents()
        {
            if (string.IsNullOrWhiteSpace(TaxComponents))
                return new List<TaxComponent>();

            try
            {
                return JsonSerializer.Deserialize<List<TaxComponent>>(TaxComponents) ?? new List<TaxComponent>();
            }
            catch
            {
                return new List<TaxComponent>();
            }
        }

        public void SetTaxComponents(List<TaxComponent> components)
        {
            TaxComponents = JsonSerializer.Serialize(components);
        }
    }

    public class TaxComponent
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsCentrallyGoverned { get; set; }
        public string? Description { get; set; }
    }
}

