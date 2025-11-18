using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Application.Localization.Dtos;

namespace CRM.Application.Localization.Services;

public interface IExchangeRateService
{
    Task<List<ExchangeRateDto>> GetLatestRatesAsync();
    Task<ExchangeRateDto?> GetRateAsync(string fromCurrencyCode, string toCurrencyCode, DateTime? asOfDate = null);
    Task<List<ExchangeRateDto>> GetHistoricalRatesAsync(string fromCurrencyCode, string toCurrencyCode, DateTime fromDate, DateTime toDate);
    Task<ExchangeRateDto> UpdateExchangeRateAsync(UpdateExchangeRateRequest request);
}


