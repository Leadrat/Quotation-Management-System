using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Application.Localization.Dtos;
using CRM.Application.Localization.Services;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Localization.Queries.Handlers;

public class GetSupportedCurrenciesQueryHandler
{
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<GetSupportedCurrenciesQueryHandler> _logger;

    public GetSupportedCurrenciesQueryHandler(ICurrencyService currencyService, ILogger<GetSupportedCurrenciesQueryHandler> logger)
    {
        _currencyService = currencyService;
        _logger = logger;
    }

    public async Task<List<CurrencyDto>> Handle(GetSupportedCurrenciesQuery query)
    {
        try
        {
            return await _currencyService.GetSupportedCurrenciesAsync();
        }
        catch (Exception ex)
        {
            // Check if this is a missing table error (check both outer and inner exceptions)
            var exceptionMessage = ex.Message;
            var innerException = ex.InnerException;
            while (innerException != null)
            {
                exceptionMessage += " | " + innerException.Message;
                innerException = innerException.InnerException;
            }

            if (exceptionMessage.Contains("42P01") || 
                exceptionMessage.Contains("does not exist") || 
                (exceptionMessage.Contains("relation") && exceptionMessage.Contains("not exist")) ||
                exceptionMessage.Contains("Invalid object name") ||
                exceptionMessage.Contains("could not be found"))
            {
                _logger.LogWarning("Currencies table does not exist, returning empty list");
                return new List<CurrencyDto>();
            }

            _logger.LogError(ex, "Error getting supported currencies");
            throw;
        }
    }
}


