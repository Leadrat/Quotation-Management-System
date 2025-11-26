using System;
using CRM.Domain.Enums;

namespace CRM.Application.Products.Commands
{
    public class AddProductToQuotationCommand
    {
        public Guid QuotationId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public BillingCycle? BillingCycle { get; set; }
        public decimal? Hours { get; set; }
        public Guid? TaxCategoryId { get; set; }
        public Guid AddedByUserId { get; set; }
    }
}

