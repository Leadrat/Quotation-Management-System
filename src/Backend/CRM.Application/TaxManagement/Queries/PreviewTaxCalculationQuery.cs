using System;
using System.Collections.Generic;
using CRM.Application.TaxManagement.Services;

namespace CRM.Application.TaxManagement.Queries
{
    public class PreviewTaxCalculationQuery
    {
        public Guid ClientId { get; set; }
        public List<LineItemTaxInput> LineItems { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime CalculationDate { get; set; } = DateTime.UtcNow;
        public Guid? CountryId { get; set; }
    }
}

