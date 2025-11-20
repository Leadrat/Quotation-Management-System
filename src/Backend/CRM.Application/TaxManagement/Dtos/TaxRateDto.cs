using System;
using System.Collections.Generic;

namespace CRM.Application.TaxManagement.Dtos
{
    public class TaxRateDto
    {
        public Guid TaxRateId { get; set; }
        public Guid? JurisdictionId { get; set; }
        public Guid TaxFrameworkId { get; set; }
        public Guid? ProductServiceCategoryId { get; set; }
        public decimal TaxRate { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public bool IsExempt { get; set; }
        public bool IsZeroRated { get; set; }
        public List<TaxComponentRateDto> TaxComponents { get; set; } = new();
        public string? Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string? JurisdictionName { get; set; }
        public string? CategoryName { get; set; }
        public string? FrameworkName { get; set; }
    }

    public class TaxComponentRateDto
    {
        public string Component { get; set; } = string.Empty;
        public decimal Rate { get; set; }
    }
}

