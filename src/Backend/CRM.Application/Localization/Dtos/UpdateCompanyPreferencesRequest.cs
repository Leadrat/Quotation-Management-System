namespace CRM.Application.Localization.Dtos;

public class UpdateCompanyPreferencesRequest
{
    public string? DefaultLanguageCode { get; set; }
    public string? DefaultCurrencyCode { get; set; }
    public string? DateFormat { get; set; }
    public string? TimeFormat { get; set; }
    public string? NumberFormat { get; set; }
    public string? Timezone { get; set; }
    public int? FirstDayOfWeek { get; set; }
}


