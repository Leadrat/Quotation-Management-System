using System;
using System.Collections.Generic;

namespace CRM.Application.TaxManagement.Requests
{
    public class PreviewTaxCalculationRequest
    {
        public Guid ClientId { get; set; }
        public List<LineItemTaxInputRequest> LineItems { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime? CalculationDate { get; set; }
        public Guid? CountryId { get; set; }
    }

    public class LineItemTaxInputRequest
    {
        public Guid LineItemId { get; set; }
        public Guid? ProductServiceCategoryId { get; set; }
        public decimal Amount { get; set; }
    }
}

