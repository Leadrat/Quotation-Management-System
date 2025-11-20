using System.Security.Claims;
using CRM.Application.CompanyDetails.Commands;
using CRM.Application.CompanyDetails.Commands.Handlers;
using CRM.Application.CompanyDetails.Dtos;
using CRM.Application.CompanyDetails.Queries;
using CRM.Application.CompanyDetails.Queries.Handlers;
using CRM.Application.CompanyDetails.Validators;
using CRM.Infrastructure.Admin.FileStorage;
using CRM.Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/company-details")]
    [Authorize(Roles = "Admin")]
    public class CompanyDetailsController : ControllerBase
    {
        private readonly GetCompanyDetailsQueryHandler _getHandler;
        private readonly UpdateCompanyDetailsCommandHandler _updateHandler;
        private readonly IFileStorageService _fileStorage;
        private readonly IAuditLogger _audit;

        public CompanyDetailsController(
            GetCompanyDetailsQueryHandler getHandler,
            UpdateCompanyDetailsCommandHandler updateHandler,
            IFileStorageService fileStorage,
            IAuditLogger audit)
        {
            _getHandler = getHandler;
            _updateHandler = updateHandler;
            _fileStorage = fileStorage;
            _audit = audit;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var query = new GetCompanyDetailsQuery();
            var result = await _getHandler.Handle(query);
            return Ok(new { success = true, data = result });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCompanyDetailsRequest request)
        {
            // Validate request
            var validator = new UpdateCompanyDetailsRequestValidator();
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

            // Create command
            var command = new UpdateCompanyDetailsCommand
            {
                Request = request,
                UpdatedBy = userId,
                IpAddress = ipAddress
            };

            // Execute handler
            var result = await _updateHandler.Handle(command);

            // Audit log
            await _audit.LogAsync("company_details_update_success", new
            {
                CompanyDetailsId = result.CompanyDetailsId,
                UpdatedBy = userId
            });

            return Ok(new { success = true, message = "Company details updated successfully", data = result });
        }

        [HttpPost("logo")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file uploaded" });
            }

            // Validate file type
            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".svg", ".webp" };
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
                    "company-logos");

                // Get IP address
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                // Update company details with logo URL
                var request = new UpdateCompanyDetailsRequest
                {
                    LogoUrl = logoUrl
                };

                var command = new UpdateCompanyDetailsCommand
                {
                    Request = request,
                    UpdatedBy = userId,
                    IpAddress = ipAddress
                };

                // Get existing company details first to preserve other fields
                var existingQuery = new GetCompanyDetailsQuery();
                var existing = await _getHandler.Handle(existingQuery);
                
                // Merge logo URL with existing data
                request.PanNumber = existing.PanNumber;
                request.TanNumber = existing.TanNumber;
                request.GstNumber = existing.GstNumber;
                request.CompanyName = existing.CompanyName;
                request.CompanyAddress = existing.CompanyAddress;
                request.City = existing.City;
                request.State = existing.State;
                request.PostalCode = existing.PostalCode;
                request.Country = existing.Country;
                request.ContactEmail = existing.ContactEmail;
                request.ContactPhone = existing.ContactPhone;
                request.Website = existing.Website;
                request.LegalDisclaimer = existing.LegalDisclaimer;
                request.BankDetails = existing.BankDetails;

                var result = await _updateHandler.Handle(command);
                return Ok(new { success = true, message = "Logo uploaded successfully", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error uploading logo: {ex.Message}" });
            }
        }
    }
}

