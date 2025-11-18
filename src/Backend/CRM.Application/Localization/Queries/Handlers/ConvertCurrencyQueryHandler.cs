using System.Threading.Tasks;
using CRM.Application.Localization.Dtos;
using CRM.Application.Localization.Services;

namespace CRM.Application.Localization.Queries.Handlers;

public class ConvertCurrencyQueryHandler
{
    private readonly ICurrencyService _currencyService;
    private readonly ILocaleFormatter _formatter;

    public ConvertCurrencyQueryHandler(ICurrencyService currencyService, ILocaleFormatter formatter)
    {
        _currencyService = currencyService;
        _formatter = formatter;
    }

    public async Task<CurrencyConversionResponse> Handle(ConvertCurrencyQuery query)
    {
        var request = query.Request;
        var result = await _currencyService.ConvertCurrencyAsync(
            request.Amount,
            request.FromCurrencyCode,
            request.ToCurrencyCode,
            request.AsOfDate);

        // Format amounts
        var fromCurrency = await _currencyService.GetCurrencyByCodeAsync(request.FromCurrencyCode);
        var toCurrency = await _currencyService.GetCurrencyByCodeAsync(request.ToCurrencyCode);

        if (fromCurrency != null)
        {
            result.FormattedOriginalAmount = _formatter.FormatCurrency(
                result.OriginalAmount,
                fromCurrency.CurrencyCode,
                "en-US"); // Use standard locale for currency formatting
        }

        if (toCurrency != null)
        {
            result.FormattedConvertedAmount = _formatter.FormatCurrency(
                result.ConvertedAmount,
                toCurrency.CurrencyCode,
                "en-US");
        }

        return result;
    }
}


