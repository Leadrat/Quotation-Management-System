namespace CRM.Application.Admin.Requests;

/// <summary>
/// Request to update branding configuration
/// </summary>
public class UpdateBrandingRequest
{
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? FooterHtml { get; set; }
}

