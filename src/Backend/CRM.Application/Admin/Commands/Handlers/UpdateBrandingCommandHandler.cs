using System.Text.Json;
using CRM.Application.Admin.DTOs;
using CRM.Application.Admin.Services;
using CRM.Application.Common.Persistence;

namespace CRM.Application.Admin.Commands.Handlers;

public class UpdateBrandingCommandHandler
{
    private readonly IBrandingService _brandingService;
    private readonly IAuditLogService _auditLogService;
    private readonly IAppDbContext _db;

    public UpdateBrandingCommandHandler(
        IBrandingService brandingService,
        IAuditLogService auditLogService,
        IAppDbContext db)
    {
        _brandingService = brandingService;
        _auditLogService = auditLogService;
        _db = db;
    }

    public async Task<CustomBrandingDto> Handle(UpdateBrandingCommand command)
    {
        // Get old branding for audit
        var oldBranding = await _brandingService.GetBrandingAsync();

        var result = await _brandingService.UpdateBrandingAsync(
            command.PrimaryColor,
            command.SecondaryColor,
            command.AccentColor,
            command.FooterHtml,
            command.UpdatedBy);

        // Log to audit
        var changes = JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            before = oldBranding != null ? new
            {
                oldBranding.PrimaryColor,
                oldBranding.SecondaryColor,
                oldBranding.AccentColor,
                hasFooterHtml = !string.IsNullOrEmpty(oldBranding.FooterHtml)
            } : null,
            after = new
            {
                result.PrimaryColor,
                result.SecondaryColor,
                result.AccentColor,
                hasFooterHtml = !string.IsNullOrEmpty(result.FooterHtml)
            },
            action = "updated"
        }));

        await _auditLogService.LogAsync(
            actionType: "BrandingUpdated",
            entity: "CustomBranding",
            entityId: result.Id,
            performedBy: command.UpdatedBy,
            ipAddress: command.IpAddress,
            changes: changes);

        return result;
    }
}

