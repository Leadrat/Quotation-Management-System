namespace CRM.Domain.Admin;

/// <summary>
/// Company-specific branding configuration
/// </summary>
public class CustomBranding
{
    public Guid Id { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; } // Hex color code
    public string? SecondaryColor { get; set; } // Hex color code
    public string? AccentColor { get; set; } // Hex color code
    public string? FooterHtml { get; set; } // Sanitized HTML
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }

    // Navigation property
    public Entities.User? UpdatedByUser { get; set; }
}

