using System;
using System.Collections.Generic;

namespace CRM.Application.TaxManagement.Commands
{
    public class CreateTaxRateCommand
    {
        public Guid? JurisdictionId { get; set; }
        public Guid TaxFrameworkId { get; set; }
        public Guid? ProductServiceCategoryId { get; set; }
        public decimal TaxRate { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public bool IsExempt { get; set; } = false;
        public bool IsZeroRated { get; set; } = false;
        public List<TaxComponentRateCommand> TaxComponents { get; set; } = new();
        public string? Description { get; set; }
        public Guid CreatedByUserId { get; set; }
    }

    public class TaxComponentRateCommand
    {
        public string Component { get; set; } = string.Empty;
        public decimal Rate { get; set; }
    }
}

