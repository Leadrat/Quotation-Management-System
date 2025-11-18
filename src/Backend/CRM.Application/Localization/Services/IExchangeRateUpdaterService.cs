using System.Threading.Tasks;

namespace CRM.Application.Localization.Services;

public interface IExchangeRateUpdaterService
{
    Task<bool> FetchLatestRatesFromApiAsync();
    Task UpdateRatesAsync();
}


