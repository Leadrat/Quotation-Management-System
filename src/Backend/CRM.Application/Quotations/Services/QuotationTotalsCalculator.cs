using System.Collections.Generic;
using System.Linq;
using CRM.Domain.Entities;

namespace CRM.Application.Quotations.Services
{
    public class QuotationTotalsCalculator
    {
        public QuotationTotalsResult Calculate(Quotation quotation, List<QuotationLineItem> lineItems, decimal discountPercentage)
        {
            // Calculate subtotal from line items (including product-level discounts)
            // Product-level discounts are already applied in the Amount field
            var subtotal = lineItems.Sum(item => item.Amount);

            // Calculate quotation-level discount (applied after product-level discounts)
            var quotationDiscountAmount = subtotal * (discountPercentage / 100m);
            if (quotationDiscountAmount > subtotal)
            {
                quotationDiscountAmount = subtotal; // Discount cannot exceed subtotal
            }

            // Calculate total product-level discounts
            var productLevelDiscounts = lineItems
                .Where(item => item.DiscountAmount.HasValue && item.DiscountAmount.Value > 0)
                .Sum(item => item.DiscountAmount.Value);

            // Total discount = product-level discounts + quotation-level discount
            var totalDiscountAmount = productLevelDiscounts + quotationDiscountAmount;
            if (totalDiscountAmount > subtotal)
            {
                totalDiscountAmount = subtotal;
            }

            // Tax calculation will be done separately by TaxCalculationService
            // This method just calculates subtotal and discount

            return new QuotationTotalsResult
            {
                SubTotal = subtotal,
                DiscountAmount = totalDiscountAmount,
                DiscountPercentage = discountPercentage,
                ProductLevelDiscounts = productLevelDiscounts,
                QuotationLevelDiscount = quotationDiscountAmount
            };
        }
    }

    public class QuotationTotalsResult
    {
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal ProductLevelDiscounts { get; set; }
        public decimal QuotationLevelDiscount { get; set; }
    }
}

