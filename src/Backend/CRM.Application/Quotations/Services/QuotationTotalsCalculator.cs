using System.Collections.Generic;
using System.Linq;
using CRM.Domain.Entities;

namespace CRM.Application.Quotations.Services
{
    public class QuotationTotalsCalculator
    {
        public QuotationTotalsResult Calculate(Quotation quotation, List<QuotationLineItem> lineItems, decimal discountPercentage)
        {
            // Calculate subtotal from line items
            var subtotal = lineItems.Sum(item => item.Amount);

            // Calculate discount
            var discountAmount = subtotal * (discountPercentage / 100m);
            if (discountAmount > subtotal)
            {
                discountAmount = subtotal; // Discount cannot exceed subtotal
            }

            // Tax calculation will be done separately by TaxCalculationService
            // This method just calculates subtotal and discount

            return new QuotationTotalsResult
            {
                SubTotal = subtotal,
                DiscountAmount = discountAmount,
                DiscountPercentage = discountPercentage
            };
        }
    }

    public class QuotationTotalsResult
    {
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
    }
}

