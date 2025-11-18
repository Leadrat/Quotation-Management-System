using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Application.Localization.Dtos;
using CRM.Application.Localization.Services;

namespace CRM.Application.Localization.Queries.Handlers;

public class GetExchangeRatesQueryHandler
{
    private readonly IExchangeRateService _exchangeRateService;

    public GetExchangeRatesQueryHandler(IExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }

    public async Task<List<ExchangeRateDto>> Handle(GetExchangeRatesQuery query)
    {
        if (!string.IsNullOrEmpty(query.FromCurrencyCode) && !string.IsNullOrEmpty(query.ToCurrencyCode))
        {
            var rate = await _exchangeRateService.GetRateAsync(
                query.FromCurrencyCode,
                query.ToCurrencyCode,
                query.AsOfDate);

            if (rate != null)
            {
                return new List<ExchangeRateDto> { rate };
            }
            return new List<ExchangeRateDto>();
        }

        return await _exchangeRateService.GetLatestRatesAsync();
    }
}


