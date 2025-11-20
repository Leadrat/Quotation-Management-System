using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Application.CompanyIdentifiers.Commands;
using CRM.Application.CompanyIdentifiers.Commands.Handlers;
using CRM.Application.CompanyIdentifiers.DTOs;
using CRM.Application.CompanyIdentifiers.Queries;
using CRM.Application.CompanyIdentifiers.Queries.Handlers;
using CRM.Application.CompanyIdentifiers.Validators;
using CRM.Infrastructure.Logging;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin/identifier-types")]
    [Authorize(Roles = "Admin")]
    public class IdentifierTypesController : ControllerBase
    {
        private readonly GetIdentifierTypesQueryHandler _getHandler;
        private readonly CreateIdentifierTypeCommandHandler _createHandler;
        private readonly UpdateIdentifierTypeCommandHandler _updateHandler;
        private readonly IAuditLogger _audit;

        public IdentifierTypesController(
            GetIdentifierTypesQueryHandler getHandler,
            CreateIdentifierTypeCommandHandler createHandler,
            UpdateIdentifierTypeCommandHandler updateHandler,
            IAuditLogger audit)
        {
            _getHandler = getHandler;
            _createHandler = createHandler;
            _updateHandler = updateHandler;
            _audit = audit;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = new GetIdentifierTypesQuery
                {
                    IncludeInactive = includeInactive
                };
                var result = await _getHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("identifier_types_get_error", new { error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving identifier types." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateIdentifierTypeRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
                await _audit.LogAsync("identifier_type_create_attempt", new { userId, request.Name });

                // Validate request
                var validator = new CreateIdentifierTypeRequestValidator();
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { success = false, errors = validationResult.Errors });
                }

                var command = new CreateIdentifierTypeCommand
                {
                    Request = request
                };

                var result = await _createHandler.Handle(command);
                await _audit.LogAsync("identifier_type_create_success", new { userId, result.IdentifierTypeId, result.Name });

                return StatusCode(201, new { success = true, message = "Identifier type created successfully", data = result });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("identifier_type_create_error", new { error = ex.Message });
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("identifier_type_create_error", new { error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while creating identifier type." });
            }
        }

        [HttpPut("{identifierTypeId}")]
        public async Task<IActionResult> Update([FromRoute] Guid identifierTypeId, [FromBody] UpdateIdentifierTypeRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
                await _audit.LogAsync("identifier_type_update_attempt", new { userId, identifierTypeId });

                // Validate request
                var validator = new UpdateIdentifierTypeRequestValidator();
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { success = false, errors = validationResult.Errors });
                }

                var command = new UpdateIdentifierTypeCommand
                {
                    IdentifierTypeId = identifierTypeId,
                    Request = request
                };

                var result = await _updateHandler.Handle(command);
                await _audit.LogAsync("identifier_type_update_success", new { userId, identifierTypeId });

                return Ok(new { success = true, message = "Identifier type updated successfully", data = result });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("identifier_type_update_error", new { userId = User.FindFirstValue(ClaimTypes.NameIdentifier), identifierTypeId, error = ex.Message });
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("identifier_type_update_error", new { userId = User.FindFirstValue(ClaimTypes.NameIdentifier), identifierTypeId, error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while updating identifier type." });
            }
        }
    }
}

