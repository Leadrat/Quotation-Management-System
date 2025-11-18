namespace CRM.Application.Localization.Dtos;

public class UpdateLocalizationResourceRequest
{
    public string? ResourceValue { get; set; }
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
}


