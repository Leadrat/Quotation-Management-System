using CRM.Application.Admin.DTOs;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Services;
using CRM.Domain.Admin;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Admin.Services;

public class BrandingService : IBrandingService
{
    private readonly IAppDbContext _db;
    private readonly IHtmlSanitizer _htmlSanitizer;

    public BrandingService(IAppDbContext db, IHtmlSanitizer htmlSanitizer)
    {
        _db = db;
        _htmlSanitizer = htmlSanitizer;
    }

    public async Task<CustomBrandingDto?> GetBrandingAsync()
    {
        var branding = await _db.CustomBranding.FirstOrDefaultAsync();
        if (branding == null) return null;

        return new CustomBrandingDto
        {
            Id = branding.Id,
            LogoUrl = branding.LogoUrl,
            PrimaryColor = branding.PrimaryColor,
            SecondaryColor = branding.SecondaryColor,
            AccentColor = branding.AccentColor,
            FooterHtml = branding.FooterHtml,
            UpdatedAt = branding.UpdatedAt,
            UpdatedBy = branding.UpdatedBy
        };
    }

    public async Task<CustomBrandingDto> UpdateBrandingAsync(
        string? primaryColor,
        string? secondaryColor,
        string? accentColor,
        string? footerHtml,
        Guid updatedBy)
    {
        var branding = await _db.CustomBranding.FirstOrDefaultAsync();
        var now = DateTimeOffset.UtcNow;

        if (branding == null)
        {
            // Create new branding configuration
            branding = new CustomBranding
            {
                Id = Guid.NewGuid(),
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor,
                AccentColor = accentColor,
                FooterHtml = !string.IsNullOrEmpty(footerHtml) ? _htmlSanitizer.Sanitize(footerHtml) : null,
                UpdatedAt = now,
                UpdatedBy = updatedBy
            };
            _db.CustomBranding.Add(branding);
        }
        else
        {
            // Update existing branding
            if (primaryColor != null) branding.PrimaryColor = primaryColor;
            if (secondaryColor != null) branding.SecondaryColor = secondaryColor;
            if (accentColor != null) branding.AccentColor = accentColor;
            if (footerHtml != null) branding.FooterHtml = _htmlSanitizer.Sanitize(footerHtml);
            branding.UpdatedAt = now;
            branding.UpdatedBy = updatedBy;
        }

        await _db.SaveChangesAsync();

        return new CustomBrandingDto
        {
            Id = branding.Id,
            LogoUrl = branding.LogoUrl,
            PrimaryColor = branding.PrimaryColor,
            SecondaryColor = branding.SecondaryColor,
            AccentColor = branding.AccentColor,
            FooterHtml = branding.FooterHtml,
            UpdatedAt = branding.UpdatedAt,
            UpdatedBy = branding.UpdatedBy
        };
    }

    public async Task<CustomBrandingDto> UpdateLogoAsync(string logoUrl, Guid updatedBy)
    {
        var branding = await _db.CustomBranding.FirstOrDefaultAsync();
        var now = DateTimeOffset.UtcNow;

        if (branding == null)
        {
            branding = new CustomBranding
            {
                Id = Guid.NewGuid(),
                LogoUrl = logoUrl,
                UpdatedAt = now,
                UpdatedBy = updatedBy
            };
            _db.CustomBranding.Add(branding);
        }
        else
        {
            branding.LogoUrl = logoUrl;
            branding.UpdatedAt = now;
            branding.UpdatedBy = updatedBy;
        }

        await _db.SaveChangesAsync();

        return new CustomBrandingDto
        {
            Id = branding.Id,
            LogoUrl = branding.LogoUrl,
            PrimaryColor = branding.PrimaryColor,
            SecondaryColor = branding.SecondaryColor,
            AccentColor = branding.AccentColor,
            FooterHtml = branding.FooterHtml,
            UpdatedAt = branding.UpdatedAt,
            UpdatedBy = branding.UpdatedBy
        };
    }
}

