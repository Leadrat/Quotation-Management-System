using System;
using System.Collections.Generic;

namespace CRM.Application.TaxManagement.Requests
{
    public class UpdateTaxRateRequest
    {
        public Guid? JurisdictionId { get; set; }
        public Guid TaxFrameworkId { get; set; }
        public Guid? ProductServiceCategoryId { get; set; }
        public decimal TaxRate { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public bool IsExempt { get; set; }
        public bool IsZeroRated { get; set; }
        public List<TaxComponentRateRequest> TaxComponents { get; set; } = new();
        public string? Description { get; set; }
    }
}

