using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Application.TaxManagement.Dtos;

namespace CRM.Application.TaxManagement.Services
{
    public interface ITaxCalculationService
    {
        Task<TaxCalculationResultDto> CalculateTaxAsync(
            Guid clientId,
            IEnumerable<LineItemTaxInput> lineItems,
            decimal subtotal,
            decimal discountAmount,
            DateTime calculationDate,
            Guid? countryId = null,
            CancellationToken cancellationToken = default);
    }

    public class LineItemTaxInput
    {
        public Guid LineItemId { get; set; }
        public Guid? ProductServiceCategoryId { get; set; }
        public decimal Amount { get; set; } // Amount after discount
    }
}

