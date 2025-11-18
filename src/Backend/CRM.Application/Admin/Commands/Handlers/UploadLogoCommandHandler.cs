using System.Text.Json;
using CRM.Application.Admin.DTOs;
using CRM.Application.Admin.Services;
using CRM.Application.Common.Persistence;

namespace CRM.Application.Admin.Commands.Handlers;

public class UploadLogoCommandHandler
{
    private readonly IBrandingService _brandingService;
    private readonly IAuditLogService _auditLogService;
    private readonly IAppDbContext _db;

    public UploadLogoCommandHandler(
        IBrandingService brandingService,
        IAuditLogService auditLogService,
        IAppDbContext db)
    {
        _brandingService = brandingService;
        _auditLogService = auditLogService;
        _db = db;
    }

    public async Task<CustomBrandingDto> Handle(UploadLogoCommand command)
    {
        // Get old branding for audit
        var oldBranding = await _brandingService.GetBrandingAsync();

        var result = await _brandingService.UpdateLogoAsync(command.LogoUrl, command.UpdatedBy);

        // Log to audit
        var changes = JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            before = oldBranding?.LogoUrl,
            after = result.LogoUrl,
            action = "logo_uploaded"
        }));

        await _auditLogService.LogAsync(
            actionType: "BrandingLogoUploaded",
            entity: "CustomBranding",
            entityId: result.Id,
            performedBy: command.UpdatedBy,
            ipAddress: command.IpAddress,
            changes: changes);

        return result;
    }
}

