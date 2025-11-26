using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Commands;
using CRM.Application.Quotations.Commands.Handlers;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Exceptions;
using CRM.Application.Quotations.Queries;
using CRM.Application.Quotations.Queries.Handlers;
using CRM.Application.Quotations.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/quotations")]
    [Authorize]
    public class QuotationsController : ControllerBase
    {
        private readonly CreateQuotationCommandHandler _createHandler;
        private readonly UpdateQuotationCommandHandler _updateHandler;
        private readonly DeleteQuotationCommandHandler _deleteHandler;
        private readonly SendQuotationCommandHandler _sendHandler;
        private readonly ResendQuotationCommandHandler _resendHandler;
        private readonly GetQuotationByIdQueryHandler _getByIdHandler;
        private readonly GetAllQuotationsQueryHandler _getAllHandler;
        private readonly GetQuotationsByClientQueryHandler _getByClientHandler;
        private readonly GetQuotationStatusHistoryQueryHandler _statusHistoryHandler;
        private readonly GetQuotationResponseQueryHandler _responseHandler;
        private readonly GetQuotationAccessLinkQueryHandler _accessLinkHandler;
        private readonly IAppDbContext _db;
        private readonly IQuotationPdfGenerationService _pdfService;
        private readonly Application.QuotationTemplates.Services.ITemplateProcessingService _templateProcessingService;
        private readonly IValidator<CreateQuotationRequest> _createValidator;
        private readonly IValidator<UpdateQuotationRequest> _updateValidator;
        private readonly IValidator<SendQuotationRequest> _sendValidator;

        public QuotationsController(
            CreateQuotationCommandHandler createHandler,
            UpdateQuotationCommandHandler updateHandler,
            DeleteQuotationCommandHandler deleteHandler,
            SendQuotationCommandHandler sendHandler,
            ResendQuotationCommandHandler resendHandler,
            GetQuotationByIdQueryHandler getByIdHandler,
            GetAllQuotationsQueryHandler getAllHandler,
            GetQuotationsByClientQueryHandler getByClientHandler,
            GetQuotationStatusHistoryQueryHandler statusHistoryHandler,
            GetQuotationResponseQueryHandler responseHandler,
            GetQuotationAccessLinkQueryHandler accessLinkHandler,
            IAppDbContext db,
            IQuotationPdfGenerationService pdfService,
            Application.QuotationTemplates.Services.ITemplateProcessingService templateProcessingService,
            IValidator<CreateQuotationRequest> createValidator,
            IValidator<UpdateQuotationRequest> updateValidator,
            IValidator<SendQuotationRequest> sendValidator)
        {
            _createHandler = createHandler;
            _updateHandler = updateHandler;
            _deleteHandler = deleteHandler;
            _sendHandler = sendHandler;
            _resendHandler = resendHandler;
            _getByIdHandler = getByIdHandler;
            _getAllHandler = getAllHandler;
            _getByClientHandler = getByClientHandler;
            _statusHistoryHandler = statusHistoryHandler;
            _responseHandler = responseHandler;
            _accessLinkHandler = accessLinkHandler;
            _db = db;
            _pdfService = pdfService;
            _templateProcessingService = templateProcessingService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _sendValidator = sendValidator;
        }

        [HttpGet]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        public async Task<IActionResult> GetAllQuotations(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? clientId = null,
            [FromQuery] Guid? userId = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                var role = User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

                if (!Guid.TryParse(userIdClaim, out var requestorUserId))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                // Convert DateTime parameters to UTC to avoid PostgreSQL timezone issues
                DateTime? utcDateFrom = null;
                DateTime? utcDateTo = null;
                
                if (dateFrom.HasValue)
                {
                    utcDateFrom = dateFrom.Value.Kind == DateTimeKind.Unspecified 
                        ? DateTime.SpecifyKind(dateFrom.Value, DateTimeKind.Utc) 
                        : dateFrom.Value.ToUniversalTime();
                }
                
                if (dateTo.HasValue)
                {
                    utcDateTo = dateTo.Value.Kind == DateTimeKind.Unspecified 
                        ? DateTime.SpecifyKind(dateTo.Value, DateTimeKind.Utc) 
                        : dateTo.Value.ToUniversalTime();
                }

                var query = new GetAllQuotationsQuery
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    ClientId = clientId,
                    CreatedByUserId = userId,
                    Status = status,
                    DateFrom = utcDateFrom,
                    DateTo = utcDateTo,
                    RequestorUserId = requestorUserId,
                    RequestorRole = role
                };

                var result = await _getAllHandler.Handle(query);

                return Ok(new { success = true, data = result.Data, pageNumber = result.PageNumber, pageSize = result.PageSize, totalCount = result.TotalCount });
            }
            catch (Exception ex)
            {
                // Log full exception details for debugging
                System.Diagnostics.Debug.WriteLine($"GetAllQuotations Error: {ex}");
                return StatusCode(500, new { error = ex.Message, details = ex.ToString() });
            }
        }

        [HttpGet("{quotationId}")]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        public async Task<IActionResult> GetQuotationById(Guid quotationId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                var role = User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

                if (!Guid.TryParse(userIdClaim, out var requestorUserId))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var query = new GetQuotationByIdQuery
                {
                    QuotationId = quotationId,
                    RequestorUserId = requestorUserId,
                    RequestorRole = role
                };

                var result = await _getByIdHandler.Handle(query);

                return Ok(new { success = true, data = result });
            }
            catch (QuotationNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
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

        [HttpGet("client/{clientId}")]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        public async Task<IActionResult> GetQuotationsByClient(Guid clientId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                var role = User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

                if (!Guid.TryParse(userIdClaim, out var requestorUserId))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var query = new GetQuotationsByClientQuery
                {
                    ClientId = clientId,
                    RequestorUserId = requestorUserId,
                    RequestorRole = role
                };

                var result = await _getByClientHandler.Handle(query);

                return Ok(new { success = true, data = result.ToArray() });
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

        [HttpPost("{quotationId}/send")]
        [Authorize(Roles = "SalesRep")]
        public async Task<IActionResult> SendQuotation(Guid quotationId, [FromBody] SendQuotationRequest request)
        {
            var validation = await _sendValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return BadRequest(new { errors = validation.Errors });
            }

            if (!TryGetUserContext(out var userId, out _))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var command = new SendQuotationCommand
            {
                QuotationId = quotationId,
                RequestedByUserId = userId,
                Request = request
            };

            try
            {
                var result = await _sendHandler.Handle(command);
                return Ok(new { success = true, message = "Quotation sent successfully", data = result });
            }
            catch (QuotationNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidQuotationStatusException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? "";
                var fullMessage = $"{dbEx.Message} | {innerMessage}";
                
                // Check for missing tables
                if (fullMessage.Contains("42P01") || 
                    fullMessage.Contains("does not exist") || 
                    (fullMessage.Contains("relation") && fullMessage.Contains("not exist")))
                {
                    return StatusCode(503, new { error = "Database table missing. Please contact administrator to run migrations.", details = fullMessage });
                }
                
                // Check for foreign key violations
                if (fullMessage.Contains("foreign key") || fullMessage.Contains("violates foreign key constraint"))
                {
                    return BadRequest(new { error = "Invalid reference. Please check that all referenced entities exist.", details = fullMessage });
                }
                
                return StatusCode(500, new { error = "Failed to save quotation data. Please try again.", details = innerMessage });
            }
            catch (InvalidOperationException ex)
            {
                var exceptionMessage = ex.Message;
                if (exceptionMessage.Contains("does not exist") || exceptionMessage.Contains("table"))
                {
                    return StatusCode(503, new { error = "Database table missing. Please contact administrator.", details = exceptionMessage });
                }
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                var exceptionMessage = ex.Message;
                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    exceptionMessage += " | " + innerException.Message;
                    innerException = innerException.InnerException;
                }
                
                // Check for missing tables
                if (exceptionMessage.Contains("42P01") || 
                    exceptionMessage.Contains("does not exist") || 
                    (exceptionMessage.Contains("relation") && exceptionMessage.Contains("not exist")))
                {
                    return StatusCode(503, new { error = "Database table missing. Please contact administrator to run migrations.", details = exceptionMessage });
                }
                
                return StatusCode(500, new { error = "An error occurred while sending the quotation. Please try again.", details = exceptionMessage });
            }
        }

        [HttpPost("{quotationId}/resend")]
        [Authorize(Roles = "SalesRep")]
        public async Task<IActionResult> ResendQuotation(Guid quotationId, [FromBody] SendQuotationRequest request)
        {
            var validation = await _sendValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return BadRequest(new { errors = validation.Errors });
            }

            if (!TryGetUserContext(out var userId, out _))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var command = new ResendQuotationCommand
            {
                QuotationId = quotationId,
                RequestedByUserId = userId,
                Request = request
            };

            try
            {
                var result = await _resendHandler.Handle(command);
                return Ok(new { success = true, message = "Quotation resent successfully", data = result });
            }
            catch (QuotationNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidQuotationStatusException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? "";
                var fullMessage = $"{dbEx.Message} | {innerMessage}";
                
                if (fullMessage.Contains("42P01") || 
                    fullMessage.Contains("does not exist") || 
                    (fullMessage.Contains("relation") && fullMessage.Contains("not exist")))
                {
                    return StatusCode(503, new { error = "Database table missing. Please contact administrator to run migrations.", details = fullMessage });
                }
                
                if (fullMessage.Contains("foreign key") || fullMessage.Contains("violates foreign key constraint"))
                {
                    return BadRequest(new { error = "Invalid reference. Please check that all referenced entities exist.", details = fullMessage });
                }
                
                return StatusCode(500, new { error = "Failed to save quotation data. Please try again.", details = innerMessage });
            }
            catch (InvalidOperationException ex)
            {
                var exceptionMessage = ex.Message;
                if (exceptionMessage.Contains("does not exist") || exceptionMessage.Contains("table"))
                {
                    return StatusCode(503, new { error = "Database table missing. Please contact administrator.", details = exceptionMessage });
                }
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                var exceptionMessage = ex.Message;
                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    exceptionMessage += " | " + innerException.Message;
                    innerException = innerException.InnerException;
                }
                
                if (exceptionMessage.Contains("42P01") || 
                    exceptionMessage.Contains("does not exist") || 
                    (exceptionMessage.Contains("relation") && exceptionMessage.Contains("not exist")))
                {
                    return StatusCode(503, new { error = "Database table missing. Please contact administrator to run migrations.", details = exceptionMessage });
                }
                
                return StatusCode(500, new { error = "An error occurred while resending the quotation. Please try again.", details = exceptionMessage });
            }
        }

        [HttpGet("{quotationId}/status-history")]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        public async Task<IActionResult> GetStatusHistory(Guid quotationId)
        {
            if (!TryGetUserContext(out var userId, out var role))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var query = new GetQuotationStatusHistoryQuery
            {
                QuotationId = quotationId,
                RequestorUserId = userId,
                RequestorRole = role
            };

            try
            {
                var result = await _statusHistoryHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (QuotationNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpGet("{quotationId}/response")]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        public async Task<IActionResult> GetResponse(Guid quotationId)
        {
            if (!TryGetUserContext(out var userId, out var role))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var query = new GetQuotationResponseQuery
            {
                QuotationId = quotationId,
                RequestorUserId = userId,
                RequestorRole = role
            };

            try
            {
                var result = await _responseHandler.Handle(query);
                return result == null
                    ? NoContent()
                    : Ok(new { success = true, data = result });
            }
            catch (QuotationNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpGet("{quotationId}/access-link")]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        public async Task<IActionResult> GetAccessLink(Guid quotationId)
        {
            if (!TryGetUserContext(out var userId, out var role))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var query = new GetQuotationAccessLinkQuery
            {
                QuotationId = quotationId,
                RequestorUserId = userId,
                RequestorRole = role
            };

            try
            {
                var result = await _accessLinkHandler.Handle(query);
                return result == null
                    ? NoContent()
                    : Ok(new { success = true, data = result });
            }
            catch (QuotationNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpGet("{quotationId:guid}/template-preview")]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        public async Task<IActionResult> GetQuotationTemplatePreview(Guid quotationId)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out var role))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var quotation = await _db.Quotations
                    .Include(q => q.Client)
                    .Include(q => q.CreatedByUser)
                    .Include(q => q.LineItems)
                    .FirstOrDefaultAsync(q => q.QuotationId == quotationId);

                if (quotation == null)
                {
                    return NotFound(new { error = "Quotation not found" });
                }

                var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
                var isManager = string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase);

                if (!isAdmin && !isManager && quotation.CreatedByUserId != userId)
                {
                    return Forbid("You do not have permission to view this quotation's template preview.");
                }

                if (!quotation.TemplateId.HasValue)
                {
                    return Ok(new { success = true, data = new { hasTemplate = false, message = "No template applied to this quotation." } });
                }

                var template = await _db.QuotationTemplates
                    .FirstOrDefaultAsync(t => t.TemplateId == quotation.TemplateId.Value);

                if (template == null || !template.IsFileBased)
                {
                    return Ok(new { success = true, data = new { hasTemplate = false, message = "Applied template not found or is not file-based." } });
                }

                var htmlContent = await _templateProcessingService.ProcessTemplateToHtmlAsync(template, quotation);

                return Ok(new { success = true, data = new { hasTemplate = true, templateName = template.Name, content = htmlContent } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get template preview", details = ex.Message });
            }
        }

        [HttpGet("{quotationId}/download-pdf")]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        public async Task<IActionResult> DownloadPdf(Guid quotationId)
        {
            if (!TryGetUserContext(out var userId, out var role))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

            var quotation = await _db.Quotations
                .Include(q => q.Client)
                .Include(q => q.CreatedByUser)
                .Include(q => q.LineItems)
                .FirstOrDefaultAsync(q => q.QuotationId == quotationId);

            if (quotation == null)
            {
                return NotFound(new { error = "Quotation not found" });
            }

            var isManager = string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase);
            
            // Admin and Manager can view all quotations, SalesRep can only view their own
            if (!isAdmin && !isManager && quotation.CreatedByUserId != userId)
            {
                return Forbid("You do not have permission to access this quotation.");
            }

            var pdfBytes = await _pdfService.GenerateQuotationPdfAsync(quotation);
            var fileName = $"Quotation-{quotation.QuotationNumber}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        [HttpGet("{quotationId}/download-docx")]
        [Authorize(Roles = "SalesRep,Manager,Admin")]
        public async Task<IActionResult> DownloadDocx(Guid quotationId)
        {
            if (!TryGetUserContext(out var userId, out var role))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            var isManager = string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase);

            var quotation = await _db.Quotations
                .Include(q => q.Client)
                .Include(q => q.CreatedByUser)
                .Include(q => q.LineItems)
                .FirstOrDefaultAsync(q => q.QuotationId == quotationId);

            if (quotation == null)
            {
                return NotFound(new { error = "Quotation not found" });
            }

            // Admin and Manager can view all quotations, SalesRep can only view their own
            if (!isAdmin && !isManager && quotation.CreatedByUserId != userId)
            {
                return Forbid("You do not have permission to access this quotation.");
            }

            byte[] docxBytes;
            if (quotation.TemplateId.HasValue)
            {
                var template = await _db.QuotationTemplates
                    .FirstOrDefaultAsync(t => t.TemplateId == quotation.TemplateId.Value);

                if (template != null && template.IsFileBased && (
                    (template.MimeType != null && template.MimeType.Contains("word", StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(template.FileName) && (template.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) || template.FileName.EndsWith(".doc", StringComparison.OrdinalIgnoreCase)))))
                {
                    docxBytes = await _templateProcessingService.ProcessTemplateToDocxAsync(template, quotation);
                }
                else
                {
                    docxBytes = await _templateProcessingService.GenerateQuotationDocxAsync(quotation);
                }
            }
            else
            {
                docxBytes = await _templateProcessingService.GenerateQuotationDocxAsync(quotation);
            }

            var fileName = $"Quotation-{quotation.QuotationNumber}.docx";
            return File(docxBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
        }

        [HttpPost]
        [Authorize(Roles = "SalesRep")]
        public async Task<IActionResult> CreateQuotation([FromBody] CreateQuotationRequest request)
        {
            try
            {
                var validationResult = await _createValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { errors = validationResult.Errors });
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                if (!Guid.TryParse(userIdClaim, out var createdByUserId))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var command = new CreateQuotationCommand
                {
                    Request = request,
                    CreatedByUserId = createdByUserId
                };

                var result = await _createHandler.Handle(command);

                return StatusCode(201, new { success = true, message = "Quotation created successfully", data = result });
            }
            catch (InvalidOperationException ex)
            {
                // Check if it's a missing table error
                if (ex.Message.Contains("does not exist") || ex.Message.Contains("table"))
                {
                    return StatusCode(503, new { error = "Database table missing. Please contact administrator to run migrations.", details = ex.Message });
                }
                // Check if it's a database save error
                if (ex.Message.Contains("Failed to save quotation") || ex.Message.Contains("saving entity changes"))
                {
                    return StatusCode(500, new { error = "Failed to create quotation. Please check that the client exists and all required fields are valid.", details = ex.Message });
                }
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Extract detailed error message
                var errorMessage = dbEx.Message;
                var innerEx = dbEx.InnerException;
                while (innerEx != null)
                {
                    errorMessage += " | " + innerEx.Message;
                    innerEx = innerEx.InnerException;
                }

                // Check for foreign key violations
                if (errorMessage.Contains("foreign key") || errorMessage.Contains("violates foreign key constraint"))
                {
                    if (errorMessage.Contains("ClientId") || errorMessage.Contains("Clients"))
                    {
                        return BadRequest(new { error = "Invalid client. The selected client does not exist.", details = errorMessage });
                    }
                    if (errorMessage.Contains("CreatedByUserId") || errorMessage.Contains("Users"))
                    {
                        return BadRequest(new { error = "Invalid user. Please log in again.", details = errorMessage });
                    }
                    return BadRequest(new { error = "Invalid reference. Please check that all referenced entities exist.", details = errorMessage });
                }

                // Check for unique constraint violations
                if (errorMessage.Contains("unique constraint") || errorMessage.Contains("duplicate key") || errorMessage.Contains("QuotationNumber"))
                {
                    return Conflict(new { error = "A quotation with this number already exists. Please try again.", details = errorMessage });
                }

                return StatusCode(500, new { error = "Failed to create quotation. Please check all fields and try again.", details = errorMessage });
            }
            catch (Exception ex)
            {
                // Check if this is a missing table error
                var exceptionMessage = ex.Message;
                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    exceptionMessage += " | " + innerException.Message;
                    innerException = innerException.InnerException;
                }

                if (exceptionMessage.Contains("42P01") || 
                    exceptionMessage.Contains("does not exist") || 
                    (exceptionMessage.Contains("relation") && exceptionMessage.Contains("not exist")) ||
                    exceptionMessage.Contains("Quotations"))
                {
                    return StatusCode(503, new { error = "Database table missing. Please contact administrator to run migrations.", details = exceptionMessage });
                }

                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("{quotationId}")]
        [Authorize(Roles = "SalesRep")]
        public async Task<IActionResult> UpdateQuotation(Guid quotationId, [FromBody] UpdateQuotationRequest request)
        {
            try
            {
                var validationResult = await _updateValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { errors = validationResult.Errors });
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                var role = User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

                if (!Guid.TryParse(userIdClaim, out var updatedByUserId))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var command = new UpdateQuotationCommand
                {
                    QuotationId = quotationId,
                    Request = request,
                    UpdatedByUserId = updatedByUserId,
                    RequestorRole = role
                };

                var result = await _updateHandler.Handle(command);

                return Ok(new { success = true, message = "Quotation updated successfully", data = result });
            }
            catch (QuotationNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidQuotationStatusException ex)
            {
                return BadRequest(new { error = ex.Message });
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

        [HttpDelete("{quotationId}")]
        [Authorize(Roles = "SalesRep")]
        public async Task<IActionResult> DeleteQuotation(Guid quotationId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                var role = User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

                if (!Guid.TryParse(userIdClaim, out var deletedByUserId))
                {
                    return Unauthorized(new { error = "Invalid user token" });
                }

                var command = new DeleteQuotationCommand
                {
                    QuotationId = quotationId,
                    DeletedByUserId = deletedByUserId,
                    RequestorRole = role
                };

                await _deleteHandler.Handle(command);

                return Ok(new { success = true, message = "Quotation deleted successfully" });
            }
            catch (QuotationNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidQuotationStatusException ex)
            {
                return BadRequest(new { error = ex.Message });
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

        private bool TryGetUserContext(out Guid userId, out string role)
        {
            userId = Guid.Empty;
            role = User.FindFirstValue("role") ?? string.Empty;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

            return Guid.TryParse(userIdClaim, out userId);
        }
    }
}

