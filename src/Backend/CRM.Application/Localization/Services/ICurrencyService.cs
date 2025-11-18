using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Application.Localization.Dtos;

namespace CRM.Application.Localization.Services;

public interface ICurrencyService
{
    Task<List<CurrencyDto>> GetSupportedCurrenciesAsync();
    Task<CurrencyDto?> GetCurrencyByCodeAsync(string code);
    Task<CurrencyConversionResponse> ConvertCurrencyAsync(decimal amount, string fromCurrencyCode, string toCurrencyCode, DateTime? asOfDate = null);
    Task<decimal?> GetExchangeRateAsync(string fromCurrencyCode, string toCurrencyCode, DateTime? asOfDate = null);
}


