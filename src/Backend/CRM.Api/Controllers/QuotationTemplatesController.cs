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

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/quotation-templates")]
    [Authorize]
    public class QuotationTemplatesController : ControllerBase
    {
        private readonly CreateQuotationTemplateCommandHandler _createHandler;
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

        public QuotationTemplatesController(
            CreateQuotationTemplateCommandHandler createHandler,
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
            IValidator<ApplyTemplateToQuotationCommand> applyValidator)
        {
            _createHandler = createHandler;
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
        /// Create a new quotation template
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SalesRep,Admin")]
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
        [Authorize(Roles = "SalesRep,Admin")]
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
        [Authorize(Roles = "SalesRep,Admin")]
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
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get a quotation template by ID
        /// </summary>
        [HttpGet("{templateId}")]
        [Authorize(Roles = "SalesRep,Admin")]
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
        [Authorize(Roles = "SalesRep,Admin")]
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
        [Authorize(Roles = "SalesRep,Admin")]
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
        [Authorize(Roles = "SalesRep,Admin")]
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
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

