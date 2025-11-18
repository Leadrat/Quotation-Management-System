using CRM.Application.Admin.DTOs;

namespace CRM.Application.Admin.Services;

/// <summary>
/// Service for managing custom branding
/// </summary>
public interface IBrandingService
{
    /// <summary>
    /// Gets the current branding configuration
    /// </summary>
    Task<CustomBrandingDto?> GetBrandingAsync();

    /// <summary>
    /// Updates branding configuration
    /// </summary>
    Task<CustomBrandingDto> UpdateBrandingAsync(
        string? primaryColor,
        string? secondaryColor,
        string? accentColor,
        string? footerHtml,
        Guid updatedBy);

    /// <summary>
    /// Updates logo URL
    /// </summary>
    Task<CustomBrandingDto> UpdateLogoAsync(string logoUrl, Guid updatedBy);
}

