namespace CRM.Application.Localization.Queries;

public class GetLocalizationResourcesQuery
{
    public string LanguageCode { get; set; } = string.Empty;
    public string? Category { get; set; }
}


