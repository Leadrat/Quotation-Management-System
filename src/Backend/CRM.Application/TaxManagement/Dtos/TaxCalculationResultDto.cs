using System;
using System.Collections.Generic;

namespace CRM.Application.TaxManagement.Dtos
{
    public class TaxCalculationResultDto
    {
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxableAmount { get; set; }
        public List<TaxComponentBreakdownDto> TaxBreakdown { get; set; } = new();
        public decimal TotalTax { get; set; }
        public decimal TotalAmount { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? JurisdictionId { get; set; }
        public Guid? TaxFrameworkId { get; set; }
        public string? CountryName { get; set; }
        public string? JurisdictionName { get; set; }
        public string? FrameworkName { get; set; }
        public List<LineItemTaxBreakdownDto> LineItemBreakdown { get; set; } = new();
    }

    public class TaxComponentBreakdownDto
    {
        public string Component { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }

    public class LineItemTaxBreakdownDto
    {
        public Guid LineItemId { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public List<TaxComponentBreakdownDto> ComponentBreakdown { get; set; } = new();
    }
}

