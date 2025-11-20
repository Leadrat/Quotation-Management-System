using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.TaxManagement.Dtos;
using CRM.Application.TaxManagement.Queries;
using CRM.Application.TaxManagement.Services;

namespace CRM.Application.TaxManagement.Queries.Handlers
{
    public class PreviewTaxCalculationQueryHandler
    {
        private readonly ITaxCalculationService _taxCalculationService;

        public PreviewTaxCalculationQueryHandler(ITaxCalculationService taxCalculationService)
        {
            _taxCalculationService = taxCalculationService;
        }

        public async Task<TaxCalculationResultDto> Handle(PreviewTaxCalculationQuery query)
        {
            return await _taxCalculationService.CalculateTaxAsync(
                query.ClientId,
                query.LineItems,
                query.Subtotal,
                query.DiscountAmount,
                query.CalculationDate,
                query.CountryId);
        }
    }
}

