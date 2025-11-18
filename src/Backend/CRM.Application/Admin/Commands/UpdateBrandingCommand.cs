using CRM.Application.Admin.DTOs;

namespace CRM.Application.Admin.Commands;

/// <summary>
/// Command to update branding configuration
/// </summary>
public class UpdateBrandingCommand
{
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? FooterHtml { get; set; }
    public Guid UpdatedBy { get; set; }
    public string? IpAddress { get; set; }
}

