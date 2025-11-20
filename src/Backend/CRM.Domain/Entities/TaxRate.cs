using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace CRM.Domain.Entities
{
    [Table("TaxRates")]
    public class TaxRate
    {
        public Guid TaxRateId { get; set; }
        public Guid? JurisdictionId { get; set; }
        public Guid TaxFrameworkId { get; set; }
        public Guid? ProductServiceCategoryId { get; set; }
        public decimal Rate { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public bool IsExempt { get; set; } = false;
        public bool IsZeroRated { get; set; } = false;
        public string TaxComponents { get; set; } = string.Empty; // JSONB
        public string? Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation properties
        public virtual Jurisdiction? Jurisdiction { get; set; }
        public virtual TaxFramework TaxFramework { get; set; } = null!;
        public virtual ProductServiceCategory? ProductServiceCategory { get; set; }

        // Domain methods
        public bool IsEffective(DateOnly date)
        {
            return date >= EffectiveFrom && (EffectiveTo == null || date <= EffectiveTo.Value);
        }

        public List<TaxComponentRate> GetTaxComponentRates()
        {
            if (string.IsNullOrWhiteSpace(TaxComponents))
                return new List<TaxComponentRate>();

            try
            {
                return JsonSerializer.Deserialize<List<TaxComponentRate>>(TaxComponents) ?? new List<TaxComponentRate>();
            }
            catch
            {
                return new List<TaxComponentRate>();
            }
        }

        public void SetTaxComponentRates(List<TaxComponentRate> componentRates)
        {
            TaxComponents = JsonSerializer.Serialize(componentRates);
        }
    }

    public class TaxComponentRate
    {
        public string Component { get; set; } = string.Empty;
        public decimal Rate { get; set; }
    }
}

