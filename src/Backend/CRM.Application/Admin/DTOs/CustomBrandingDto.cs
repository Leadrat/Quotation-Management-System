namespace CRM.Application.Admin.DTOs;

/// <summary>
/// DTO for custom branding configuration
/// </summary>
public class CustomBrandingDto
{
    public Guid Id { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? FooterHtml { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
}

