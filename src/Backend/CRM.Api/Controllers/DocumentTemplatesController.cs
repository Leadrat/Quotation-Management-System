using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Application.DocumentTemplates.Commands;
using CRM.Application.DocumentTemplates.Commands.Handlers;
using CRM.Application.DocumentTemplates.Dtos;
using CRM.Application.DocumentTemplates.Queries;
using CRM.Application.DocumentTemplates.Queries.Handlers;
using CRM.Application.DocumentTemplates.Services;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/document-templates")]
    [Authorize]
    public class DocumentTemplatesController : ControllerBase
    {
        private readonly UploadDocumentCommandHandler _uploadHandler;
        private readonly ListTemplatesQueryHandler _listHandler;
        private readonly IValidator<UploadDocumentRequest> _uploadValidator;
        private readonly IPlaceholderIdentificationService _placeholderIdService;
        private readonly IPlaceholderMappingService _placeholderMappingService;
        private readonly ILogger<DocumentTemplatesController> _logger;
        private readonly IAppDbContext _db;

        public DocumentTemplatesController(
            UploadDocumentCommandHandler uploadHandler,
            ListTemplatesQueryHandler listHandler,
            IValidator<UploadDocumentRequest> uploadValidator,
            IPlaceholderIdentificationService placeholderIdService,
            IPlaceholderMappingService placeholderMappingService,
            IAppDbContext db,
            ILogger<DocumentTemplatesController> logger)
        {
            _uploadHandler = uploadHandler;
            _listHandler = listHandler;
            _uploadValidator = uploadValidator;
            _placeholderIdService = placeholderIdService;
            _placeholderMappingService = placeholderMappingService;
            _db = db;
            _logger = logger;
        }
        /// <summary>
        /// DTO for placeholder configuration
        /// </summary>
        public record PlaceholderDto(string placeholderName, string placeholderType, string? defaultValue);

        /// <summary>
        /// List file-based document templates with optional template type filter
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,SalesRep")]
        public async Task<IActionResult> List([FromQuery] string? templateType)
        {
            if (!TryGetUserContext(out var userId, out var role))
            {
                return Unauthorized(new { message = "Invalid user context" });
            }
            try
            {
                var query = new ListTemplatesQuery
                {
                    TemplateType = templateType,
                    RequestedByUserId = userId,
                    RequestedByRole = role
                };

                var result = await _listHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing document templates");
                return StatusCode(500, new { message = "Failed to load document templates" });
            }
        }

        /// <summary>
        /// Get detected placeholders for a file-based document template
        /// </summary>
        [HttpGet("{templateId}/placeholders")]
        [Authorize(Roles = "Admin,SalesRep")]
        public async Task<IActionResult> GetPlaceholders([FromRoute] Guid templateId)
        {
            if (!TryGetUserContext(out _, out _))
                return Unauthorized(new { message = "Invalid user context" });

            try
            {
                // First try to load existing stored placeholders
                var stored = await _db.TemplatePlaceholders
                    .Where(p => p.TemplateId == templateId)
                    .OrderBy(p => p.PositionInDocument)
                    .Select(p => new PlaceholderDto(p.PlaceholderName, p.PlaceholderType, p.DefaultValue))
                    .ToListAsync();

                if (stored != null && stored.Count > 0)
                {
                    return Ok(new { success = true, data = stored });
                }

                // Fallback: return an empty list (UI can still allow manual add),
                // or plug in identification service when document text extraction is available
                return Ok(new { success = true, data = new List<PlaceholderDto>() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting placeholders for template {TemplateId}", templateId);
                return StatusCode(500, new { message = "Failed to load placeholders" });
            }
        }

        /// <summary>
        /// Save placeholder mappings for a file-based document template
        /// </summary>
        [HttpPost("{templateId}/placeholders")]
        [Authorize(Roles = "Admin,SalesRep")]
        public async Task<IActionResult> SavePlaceholders([FromRoute] Guid templateId, [FromBody] List<PlaceholderDto> placeholders)
        {
            if (!TryGetUserContext(out _, out _))
                return Unauthorized(new { message = "Invalid user context" });

            try
            {
                // Replace existing placeholders for this template with posted set
                var existing = await _db.TemplatePlaceholders
                    .Where(p => p.TemplateId == templateId)
                    .ToListAsync();

                if (existing.Count > 0)
                {
                    _db.TemplatePlaceholders.RemoveRange(existing);
                }

                var now = DateTimeOffset.UtcNow;
                var newEntities = (placeholders ?? new List<PlaceholderDto>())
                    .Where(p => !string.IsNullOrWhiteSpace(p.placeholderName))
                    .Select(p => new CRM.Domain.Entities.TemplatePlaceholder
                    {
                        PlaceholderId = Guid.NewGuid(),
                        TemplateId = templateId,
                        PlaceholderName = p.placeholderName.Trim(),
                        PlaceholderType = string.IsNullOrWhiteSpace(p.placeholderType) ? "Other" : p.placeholderType.Trim(),
                        DefaultValue = string.IsNullOrWhiteSpace(p.defaultValue) ? null : p.defaultValue,
                        IsManuallyAdded = true,
                        CreatedAt = now,
                        UpdatedAt = now
                    })
                    .ToList();

                if (newEntities.Count > 0)
                {
                    await _db.TemplatePlaceholders.AddRangeAsync(newEntities);
                }

                await _db.SaveChangesAsync();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving placeholders for template {TemplateId}", templateId);
                return StatusCode(500, new { message = "Failed to save placeholders" });
            }
        }


        private bool TryGetUserContext(out Guid userId, out string role)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            role = User.FindFirstValue("role") ?? string.Empty;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out userId))
            {
                userId = Guid.Empty;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Upload a PDF or Word document to be converted to a template
        /// </summary>
        [HttpPost("upload")]
        [Authorize(Roles = "Admin,SalesRep")]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentRequest request)
        {
            if (!TryGetUserContext(out var userId, out var role))
            {
                return Unauthorized(new { message = "Invalid user context" });
            }

            // Validate request
            var validationResult = await _uploadValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { error = "Validation failed", errors = validationResult.Errors });
            }

            try
            {
                // Copy file stream to memory to avoid disposal issues
                byte[] fileBytes;
                using (var sourceStream = request.File.OpenReadStream())
                {
                    using var memoryStream = new System.IO.MemoryStream();
                    await sourceStream.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                var handlerStream = new System.IO.MemoryStream(fileBytes);
                handlerStream.Position = 0;

                var command = new UploadDocumentCommand
                {
                    Request = request,
                    FileStream = handlerStream,
                    FileName = request.File.FileName,
                    ContentType = request.File.ContentType,
                    FileSize = request.File.Length,
                    CreatedByUserId = userId
                };

                var result = await _uploadHandler.Handle(command);
                return Created($"/api/v1/document-templates/{result.TemplateId}", new { success = true, data = result });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error uploading document template");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error uploading document template");
                return StatusCode(500, new { message = "An error occurred while uploading the document template" });
            }
        }

        // NOTE: Convert endpoint removed - ConvertDocumentCommandHandler depends on ITemplateConversionService which doesn't exist
        // TODO: Implement template conversion feature properly
    }
}

