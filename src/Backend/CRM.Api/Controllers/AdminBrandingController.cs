using System.Security.Claims;
using CRM.Application.Admin.Commands;
using CRM.Application.Admin.Commands.Handlers;
using CRM.Application.Admin.Queries;
using CRM.Application.Admin.Queries.Handlers;
using CRM.Application.Admin.Requests;
using CRM.Application.Admin.Validators;
using CRM.Infrastructure.Admin.FileStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/v1/admin/branding")]
[Authorize(Roles = "Admin")]
public class AdminBrandingController : ControllerBase
{
    private readonly GetBrandingQueryHandler _getHandler;
    private readonly UpdateBrandingCommandHandler _updateHandler;
    private readonly UploadLogoCommandHandler _uploadLogoHandler;
    private readonly IFileStorageService _fileStorage;

    public AdminBrandingController(
        GetBrandingQueryHandler getHandler,
        UpdateBrandingCommandHandler updateHandler,
        UploadLogoCommandHandler uploadLogoHandler,
        IFileStorageService fileStorage)
    {
        _getHandler = getHandler;
        _updateHandler = updateHandler;
        _uploadLogoHandler = uploadLogoHandler;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    public async Task<IActionResult> GetBranding()
    {
        var query = new GetBrandingQuery();
        var result = await _getHandler.Handle(query);
        
        if (result == null)
        {
            return Ok(new { success = true, data = (object?)null });
        }

        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateBranding([FromBody] UpdateBrandingRequest request)
    {
        // Validate request
        var validator = new UpdateBrandingRequestValidator();
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { success = false, errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        // Get user ID from JWT
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId))
        {
            return Unauthorized(new { success = false, message = "Invalid user token" });
        }

        // Get IP address
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var command = new UpdateBrandingCommand
        {
            PrimaryColor = request.PrimaryColor,
            SecondaryColor = request.SecondaryColor,
            AccentColor = request.AccentColor,
            FooterHtml = request.FooterHtml,
            UpdatedBy = userId,
            IpAddress = ipAddress
        };

        var result = await _updateHandler.Handle(command);
        return Ok(new { success = true, message = "Branding updated successfully", data = result });
    }

    [HttpPost("logo")]
    public async Task<IActionResult> UploadLogo(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "No file uploaded" });
        }

        // Validate file type
        var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".svg" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { success = false, message = $"File type {extension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}" });
        }

        // Validate file size (max 5MB)
        const long maxSize = 5 * 1024 * 1024; // 5MB
        if (file.Length > maxSize)
        {
            return BadRequest(new { success = false, message = "File size exceeds 5MB limit" });
        }

        // Get user ID from JWT
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId))
        {
            return Unauthorized(new { success = false, message = "Invalid user token" });
        }

        try
        {
            // Save file
            var logoUrl = await _fileStorage.UploadFileAsync(
                file.OpenReadStream(),
                file.FileName,
                file.ContentType,
                "branding");

            // Get IP address
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Update branding with logo URL
            var command = new UploadLogoCommand
            {
                LogoUrl = logoUrl,
                UpdatedBy = userId,
                IpAddress = ipAddress
            };

            var result = await _uploadLogoHandler.Handle(command);
            return Ok(new { success = true, message = "Logo uploaded successfully", data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Error uploading logo: {ex.Message}" });
        }
    }
}

