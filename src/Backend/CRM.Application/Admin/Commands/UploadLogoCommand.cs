using CRM.Application.Admin.DTOs;

namespace CRM.Application.Admin.Commands;

/// <summary>
/// Command to upload logo
/// </summary>
public class UploadLogoCommand
{
    public string LogoUrl { get; set; } = string.Empty;
    public Guid UpdatedBy { get; set; }
    public string? IpAddress { get; set; }
}

