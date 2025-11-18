using System.Threading.Tasks;
using CRM.Application.Localization.Dtos;
using CRM.Application.Localization.Services;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Localization.Commands.Handlers;

public class UpdateExchangeRateCommandHandler
{
    private readonly IExchangeRateService _exchangeRateService;
    private readonly ILogger<UpdateExchangeRateCommandHandler> _logger;

    public UpdateExchangeRateCommandHandler(
        IExchangeRateService exchangeRateService,
        ILogger<UpdateExchangeRateCommandHandler> logger)
    {
        _exchangeRateService = exchangeRateService;
        _logger = logger;
    }

    public async Task<ExchangeRateDto> Handle(UpdateExchangeRateCommand command)
    {
        var result = await _exchangeRateService.UpdateExchangeRateAsync(command.Request);

        _logger.LogInformation("Exchange rate updated: {FromCurrency} to {ToCurrency} = {Rate}",
            command.Request.FromCurrencyCode, command.Request.ToCurrencyCode, command.Request.Rate);
        return result;
    }
}


