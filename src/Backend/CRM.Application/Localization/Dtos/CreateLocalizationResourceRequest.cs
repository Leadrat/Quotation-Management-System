namespace CRM.Application.Localization.Dtos;

public class CreateLocalizationResourceRequest
{
    public string LanguageCode { get; set; } = string.Empty;
    public string ResourceKey { get; set; } = string.Empty;
    public string ResourceValue { get; set; } = string.Empty;
    public string? Category { get; set; }
}


