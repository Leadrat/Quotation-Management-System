namespace CRM.Application.Localization.Dtos;

public class UpdateUserPreferencesRequest
{
    public string? LanguageCode { get; set; }
    public string? CurrencyCode { get; set; }
    public string? DateFormat { get; set; }
    public string? TimeFormat { get; set; }
    public string? NumberFormat { get; set; }
    public string? Timezone { get; set; }
    public int? FirstDayOfWeek { get; set; }
}


