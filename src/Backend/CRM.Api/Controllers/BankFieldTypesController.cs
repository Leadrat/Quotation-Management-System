using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Application.CompanyBankDetails.Commands;
using CRM.Application.CompanyBankDetails.Commands.Handlers;
using CRM.Application.CompanyBankDetails.DTOs;
using CRM.Application.CompanyBankDetails.Queries;
using CRM.Application.CompanyBankDetails.Queries.Handlers;
using CRM.Application.CompanyBankDetails.Validators;
using CRM.Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin/bank-field-types")]
    [Authorize(Roles = "Admin")]
    public class BankFieldTypesController : ControllerBase
    {
        private readonly GetBankFieldTypesQueryHandler _getHandler;
        private readonly CreateBankFieldTypeCommandHandler _createHandler;
        private readonly UpdateBankFieldTypeCommandHandler _updateHandler;
        private readonly IAuditLogger _audit;

        public BankFieldTypesController(
            GetBankFieldTypesQueryHandler getHandler,
            CreateBankFieldTypeCommandHandler createHandler,
            UpdateBankFieldTypeCommandHandler updateHandler,
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
                var query = new GetBankFieldTypesQuery
                {
                    IncludeInactive = includeInactive
                };
                var result = await _getHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("bank_field_types_get_error", new { error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving bank field types." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBankFieldTypeRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
                await _audit.LogAsync("bank_field_type_create_attempt", new { userId, request.Name });

                // Validate request
                var validator = new CreateBankFieldTypeRequestValidator();
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { success = false, errors = validationResult.Errors });
                }

                var command = new CreateBankFieldTypeCommand
                {
                    Request = request
                };

                var result = await _createHandler.Handle(command);
                await _audit.LogAsync("bank_field_type_create_success", new { userId, result.BankFieldTypeId, result.Name });

                return StatusCode(201, new { success = true, message = "Bank field type created successfully", data = result });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("bank_field_type_create_error", new { error = ex.Message });
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("bank_field_type_create_error", new { error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while creating bank field type." });
            }
        }

        [HttpPut("{bankFieldTypeId}")]
        public async Task<IActionResult> Update([FromRoute] Guid bankFieldTypeId, [FromBody] UpdateBankFieldTypeRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
                await _audit.LogAsync("bank_field_type_update_attempt", new { userId, bankFieldTypeId });

                // Validate request
                var validator = new UpdateBankFieldTypeRequestValidator();
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { success = false, errors = validationResult.Errors });
                }

                var command = new UpdateBankFieldTypeCommand
                {
                    BankFieldTypeId = bankFieldTypeId,
                    Request = request
                };

                var result = await _updateHandler.Handle(command);
                await _audit.LogAsync("bank_field_type_update_success", new { userId, bankFieldTypeId });

                return Ok(new { success = true, message = "Bank field type updated successfully", data = result });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("bank_field_type_update_error", new { userId = User.FindFirstValue(ClaimTypes.NameIdentifier), bankFieldTypeId, error = ex.Message });
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("bank_field_type_update_error", new { userId = User.FindFirstValue(ClaimTypes.NameIdentifier), bankFieldTypeId, error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while updating bank field type." });
            }
        }
    }
}

