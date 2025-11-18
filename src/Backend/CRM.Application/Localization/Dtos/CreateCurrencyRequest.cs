namespace CRM.Application.Localization.Dtos;

public class CreateCurrencyRequest
{
    public string CurrencyCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; } = 2;
    public bool IsDefault { get; set; } = false;
}


