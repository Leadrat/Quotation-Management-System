using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Application.Common.Results;
using CRM.Application.QuotationTemplates.Commands;
using CRM.Application.Quotations.Dtos;
using CRM.Application.QuotationTemplates.Commands.Handlers;
using CRM.Application.QuotationTemplates.Dtos;
using CRM.Application.QuotationTemplates.Exceptions;
using CRM.Application.QuotationTemplates.Queries;
using CRM.Application.QuotationTemplates.Queries.Handlers;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/quotation-templates")]
    [Authorize]
    public class QuotationTemplatesController : ControllerBase
    {
        private readonly CreateQuotationTemplateCommandHandler _createHandler;
        private readonly UploadQuotationTemplateCommandHandler _uploadHandler;
        private readonly UpdateQuotationTemplateCommandHandler _updateHandler;
        private readonly DeleteQuotationTemplateCommandHandler _deleteHandler;
        private readonly RestoreQuotationTemplateCommandHandler _restoreHandler;
        private readonly ApproveQuotationTemplateCommandHandler _approveHandler;
        private readonly ApplyTemplateToQuotationCommandHandler _applyHandler;
        private readonly GetTemplateByIdQueryHandler _getByIdHandler;
        private readonly GetAllTemplatesQueryHandler _getAllHandler;
        private readonly GetTemplateVersionsQueryHandler _getVersionsHandler;
        private readonly GetPublicTemplatesQueryHandler _getPublicHandler;
        private readonly GetTemplateUsageStatsQueryHandler _getUsageStatsHandler;
        private readonly IValidator<CreateQuotationTemplateRequest> _createValidator;
        private readonly IValidator<UpdateQuotationTemplateRequest> _updateValidator;
        private readonly IValidator<ApproveQuotationTemplateCommand> _approveValidator;
        private readonly IValidator<ApplyTemplateToQuotationCommand> _applyValidator;
        private readonly ILogger<QuotationTemplatesController> _logger;

        public QuotationTemplatesController(
            CreateQuotationTemplateCommandHandler createHandler,
            UploadQuotationTemplateCommandHandler uploadHandler,
            UpdateQuotationTemplateCommandHandler updateHandler,
            DeleteQuotationTemplateCommandHandler deleteHandler,
            RestoreQuotationTemplateCommandHandler restoreHandler,
            ApproveQuotationTemplateCommandHandler approveHandler,
            ApplyTemplateToQuotationCommandHandler applyHandler,
            GetTemplateByIdQueryHandler getByIdHandler,
            GetAllTemplatesQueryHandler getAllHandler,
            GetTemplateVersionsQueryHandler getVersionsHandler,
            GetPublicTemplatesQueryHandler getPublicHandler,
            GetTemplateUsageStatsQueryHandler getUsageStatsHandler,
            IValidator<CreateQuotationTemplateRequest> createValidator,
            IValidator<UpdateQuotationTemplateRequest> updateValidator,
            IValidator<ApproveQuotationTemplateCommand> approveValidator,
            IValidator<ApplyTemplateToQuotationCommand> applyValidator,
            ILogger<QuotationTemplatesController> logger)
        {
            _createHandler = createHandler;
            _uploadHandler = uploadHandler;
            _updateHandler = updateHandler;
            _deleteHandler = deleteHandler;
            _restoreHandler = restoreHandler;
            _approveHandler = approveHandler;
            _applyHandler = applyHandler;
            _getByIdHandler = getByIdHandler;
            _getAllHandler = getAllHandler;
            _getVersionsHandler = getVersionsHandler;
            _getPublicHandler = getPublicHandler;
            _getUsageStatsHandler = getUsageStatsHandler;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _approveValidator = approveValidator;
            _applyValidator = applyValidator;
            _logger = logger;
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
        /// Upload a file-based quotation template
        /// </summary>
        [HttpPost("upload")]
        [Authorize(Roles = "Admin,SalesRep")]
        [ProducesResponseType(typeof(QuotationTemplateDto), 201)]
        public async Task<IActionResult> Upload([FromForm] UploadQuotationTemplateRequest request, [FromForm] IFormFile file)
        {
            try
            {
                _logger?.LogInformation("Template upload request received. File: {FileName}, Size: {FileSize}, ContentType: {ContentType}", 
                    file?.FileName, file?.Length, file?.ContentType);

                if (file == null || file.Length == 0)
                {
                    _logger?.LogWarning("Template upload failed: File is null or empty");
                    return BadRequest(new { error = "File is required" });
                }

                if (!TryGetUserContext(out var userId, out var role))
                {
                    _logger?.LogWarning("Template upload failed: Invalid user context. UserId: {UserId}, Role: {Role}", userId, role);
                    return Unauthorized(new { error = "Invalid user token" });
                }

                _logger?.LogInformation("Template upload - UserId: {UserId}, Role: {Role}, Request: {Request}", 
                    userId, role, System.Text.Json.JsonSerializer.Serialize(request));

                // Copy file stream to byte array to avoid disposal issues
                // The IFormFile stream gets disposed when the request completes,
                // so we need to copy it to memory first
                _logger?.LogInformation("Template upload - Copying file stream to memory");
                byte[] fileBytes;
                try
                {
                    using (var sourceStream = file.OpenReadStream())
                    {
                        using var memoryStream = new MemoryStream();
                        await sourceStream.CopyToAsync(memoryStream);
                        fileBytes = memoryStream.ToArray();
                        _logger?.LogInformation("Template upload - File stream copied. Size: {Size} bytes", fileBytes.Length);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Template upload - Error copying file stream: {Message}", ex.Message);
                    return StatusCode(500, new { error = "Error reading file stream", details = ex.Message });
                }

                // Create a new MemoryStream from the byte array for the handler
                // The handler will use this stream synchronously, so we need to keep it alive
                var handlerStream = new MemoryStream(fileBytes);
                handlerStream.Position = 0; // Ensure position is at the beginning
                
                _logger?.LogInformation("Template upload - Created MemoryStream. Length: {Length}, Position: {Position}", 
                    handlerStream.Length, handlerStream.Position);

                try
                {
                    _logger?.LogInformation("Template upload - Creating command and calling handler");
                    var command = new UploadQuotationTemplateCommand
                    {
                        Request = request,
                        FileStream = handlerStream,
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        FileSize = file.Length,
                        CreatedByUserId = userId
                    };
                    
                    _logger?.LogInformation("Template upload - Command created. FileName: {FileName}, FileSize: {FileSize}, ContentType: {ContentType}", 
                        command.FileName, command.FileSize, command.ContentType);

                    _logger?.LogInformation("Template upload - Calling handler. Handler type: {HandlerType}", 
                        _uploadHandler?.GetType().Name ?? "null");
                    var result = await _uploadHandler.Handle(command);
                    _logger?.LogInformation("Template upload - Handler completed successfully. TemplateId: {TemplateId}", 
                        result?.TemplateId);
                    return Created($"/api/v1/quotation-templates/{result.TemplateId}", new { success = true, data = result });
                }
                finally
                {
                    // Dispose the stream after handler completes
                    handlerStream?.Dispose();
                }
            }
            catch (ArgumentException ex)
            {
                _logger?.LogWarning(ex, "Invalid argument in template upload: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidTemplateVisibilityException ex)
            {
                _logger?.LogWarning(ex, "Invalid template visibility: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger?.LogError(ex, "Invalid operation during template upload: {Message}, InnerException: {InnerException}", 
                    ex.Message, ex.InnerException?.Message);
                
                // Check if it's a table missing error
                if (ex.Message.Contains("does not exist") || ex.Message.Contains("42P01"))
                {
                    return StatusCode(500, new { 
                        error = "Database table missing. Please run database migrations.",
                        details = ex.Message,
                        hint = "Run 'dotnet ef database update' in the CRM.Infrastructure project"
                    });
                }
                
                return BadRequest(new { error = ex.Message, details = ex.InnerException?.Message });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                _logger?.LogError(dbEx, "Database update error during template upload: {Message}, InnerException: {InnerException}", 
                    dbEx.Message, dbEx.InnerException?.Message);
                
                if (dbEx.InnerException is Npgsql.PostgresException pgEx)
                {
                    if (pgEx.SqlState == "42P01")
                    {
                        return StatusCode(500, new { 
                            error = "Database table missing. Please run database migrations.",
                            details = pgEx.Message,
                            hint = "Run 'dotnet ef database update' in the CRM.Infrastructure project"
                        });
                    }
                    return StatusCode(500, new { 
                        error = "Database error occurred", 
                        details = pgEx.Message,
                        sqlState = pgEx.SqlState
                    });
                }
                
                return StatusCode(500, new { 
                    error = "Database error occurred while uploading template", 
                    details = dbEx.Message,
                    innerException = dbEx.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error uploading template: {Message}, StackTrace: {StackTrace}, Type: {ExceptionType}", 
                    ex.Message, ex.StackTrace, ex.GetType().Name);
                return StatusCode(500, new { 
                    error = "An error occurred while uploading the template", 
                    details = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        /// <summary>
        /// Create a new quotation template
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,SalesRep")]
        [ProducesResponseType(typeof(QuotationTemplateDto), 201)]
        public async Task<IActionResult> Create([FromBody] CreateQuotationTemplateRequest request)
        {
            try
            {
                var validation = await _createValidator.ValidateAsync(request);
                if (!validation.IsValid)
                {
                    return BadRequest(new { errors = validation.Errors });
                }

                if (!TryGetUserContext(out var userId, out _))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var command = new CreateQuotationTemplateCommand
                {
                    Request = request,
                    CreatedByUserId = userId
                };

                var result = await _createHandler.Handle(command);
                return Created($"/api/v1/quotation-templates/{result.TemplateId}", new { success = true, data = result });
            }
            catch (InvalidTemplateVisibilityException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing quotation template (creates new version)
        /// </summary>
        [HttpPut("{templateId}")]
        [Authorize(Roles = "Admin,SalesRep")]
        [ProducesResponseType(typeof(QuotationTemplateDto), 200)]
        public async Task<IActionResult> Update(Guid templateId, [FromBody] UpdateQuotationTemplateRequest request)
        {
            try
            {
                var validation = await _updateValidator.ValidateAsync(request);
                if (!validation.IsValid)
                {
                    return BadRequest(new { errors = validation.Errors });
                }

                if (!TryGetUserContext(out var userId, out var role))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var command = new UpdateQuotationTemplateCommand
                {
                    TemplateId = templateId,
                    Request = request,
                    UpdatedByUserId = userId,
                    RequestorRole = role
                };

                var result = await _updateHandler.Handle(command);
                return Ok(new { success = true, data = result });
            }
            catch (QuotationTemplateNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedTemplateAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (TemplateNotEditableException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all quotation templates with pagination and filters
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,SalesRep")]
        [ProducesResponseType(typeof(PagedResult<QuotationTemplateDto>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? visibility = null,
            [FromQuery] bool? isApproved = null,
            [FromQuery] Guid? ownerUserId = null)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out var role))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var query = new GetAllTemplatesQuery
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Search = search,
                    Visibility = visibility,
                    IsApproved = isApproved,
                    OwnerUserId = ownerUserId,
                    RequestorUserId = userId,
                    RequestorRole = role
                };

                var result = await _getAllHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException concurrencyEx)
            {
                // More specific exception - catch before DbUpdateException
                return StatusCode(500, new { error = "Concurrency error occurred.", details = concurrencyEx.Message });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Database-related errors
                return StatusCode(500, new { error = "Database error occurred. Please check if the QuotationTemplates table exists and migrations are applied.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging
                var logger = HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILogger<QuotationTemplatesController>>();
                logger?.LogError(ex, "Error in GetAll templates endpoint: {Error}", ex.Message);
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Get a quotation template by ID
        /// </summary>
        [HttpGet("{templateId}")]
        [Authorize(Roles = "Admin,SalesRep")]
        [ProducesResponseType(typeof(QuotationTemplateDto), 200)]
        public async Task<IActionResult> GetById(Guid templateId)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out var role))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var query = new GetTemplateByIdQuery
                {
                    TemplateId = templateId,
                    RequestorUserId = userId,
                    RequestorRole = role
                };

                var result = await _getByIdHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (QuotationTemplateNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedTemplateAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Soft delete a quotation template
        /// </summary>
        [HttpDelete("{templateId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Delete(Guid templateId)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out var role))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var command = new DeleteQuotationTemplateCommand
                {
                    TemplateId = templateId,
                    DeletedByUserId = userId,
                    RequestorRole = role
                };

                await _deleteHandler.Handle(command);
                return Ok(new { success = true, message = "Template deleted successfully" });
            }
            catch (QuotationTemplateNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedTemplateAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Restore a deleted quotation template
        /// </summary>
        [HttpPost("{templateId}/restore")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(QuotationTemplateDto), 200)]
        public async Task<IActionResult> Restore(Guid templateId)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out var role))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var command = new RestoreQuotationTemplateCommand
                {
                    TemplateId = templateId,
                    RestoredByUserId = userId,
                    RequestorRole = role
                };

                var result = await _restoreHandler.Handle(command);
                return Ok(new { success = true, data = result });
            }
            catch (QuotationTemplateNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedTemplateAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Apply a template to create a quotation (returns CreateQuotationRequest)
        /// </summary>
        [HttpPost("{templateId}/apply")]
        [Authorize(Roles = "SalesRep,Admin")]
        [ProducesResponseType(typeof(CreateQuotationRequest), 200)]
        public async Task<IActionResult> Apply(Guid templateId, [FromQuery] Guid clientId)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out var role))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var command = new ApplyTemplateToQuotationCommand
                {
                    TemplateId = templateId,
                    ClientId = clientId,
                    AppliedByUserId = userId,
                    RequestorRole = role
                };

                var validation = await _applyValidator.ValidateAsync(command);
                if (!validation.IsValid)
                {
                    return BadRequest(new { errors = validation.Errors });
                }

                var result = await _applyHandler.Handle(command);
                return Ok(new { success = true, data = result });
            }
            catch (QuotationTemplateNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedTemplateAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (TemplateNotEditableException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get version history for a template
        /// </summary>
        [HttpGet("{templateId}/versions")]
        [Authorize(Roles = "Admin,SalesRep")]
        [ProducesResponseType(typeof(System.Collections.Generic.List<QuotationTemplateVersionDto>), 200)]
        public async Task<IActionResult> GetVersions(Guid templateId)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out _))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var query = new GetTemplateVersionsQuery
                {
                    TemplateId = templateId,
                    RequestorUserId = userId
                };

                var result = await _getVersionsHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (QuotationTemplateNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedTemplateAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Approve a template (admin only)
        /// </summary>
        [HttpPost("{templateId}/approve")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(QuotationTemplateDto), 200)]
        public async Task<IActionResult> Approve(Guid templateId)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out _))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var command = new ApproveQuotationTemplateCommand
                {
                    TemplateId = templateId,
                    ApprovedByUserId = userId
                };

                var validation = await _approveValidator.ValidateAsync(command);
                if (!validation.IsValid)
                {
                    return BadRequest(new { errors = validation.Errors });
                }

                var result = await _approveHandler.Handle(command);
                return Ok(new { success = true, data = result });
            }
            catch (QuotationTemplateNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (TemplateNotEditableException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get template usage statistics (admin only)
        /// </summary>
        [HttpGet("usage-stats")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(TemplateUsageStatsDto), 200)]
        public async Task<IActionResult> GetUsageStats(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out _))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var query = new GetTemplateUsageStatsQuery
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    RequestorUserId = userId
                };

                var result = await _getUsageStatsHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get public templates for quotation creation
        /// </summary>
        [HttpGet("public")]
        [Authorize(Roles = "SalesRep,Admin")]
        [ProducesResponseType(typeof(System.Collections.Generic.List<QuotationTemplateDto>), 200)]
        public async Task<IActionResult> GetPublic()
        {
            try
            {
                if (!TryGetUserContext(out var userId, out var role))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                // Ensure role is not empty (default to SalesRep if empty)
                if (string.IsNullOrWhiteSpace(role))
                {
                    role = "SalesRep";
                }

                var query = new GetPublicTemplatesQuery
                {
                    RequestorUserId = userId,
                    RequestorRole = role
                };

                var result = await _getPublicHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILogger<QuotationTemplatesController>>();
                logger?.LogError(ex, "Error in GetPublic templates endpoint: {Error}", ex.Message);
                return StatusCode(500, new { error = ex.Message, details = ex.StackTrace });
            }
        }
    }
}

