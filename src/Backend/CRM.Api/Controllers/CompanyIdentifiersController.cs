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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/company-details/identifiers")]
    [Authorize(Roles = "Admin")]
    public class CompanyIdentifiersController : ControllerBase
    {
        private readonly GetCompanyIdentifierValuesQueryHandler _getHandler;
        private readonly SaveCompanyIdentifierValuesCommandHandler _saveHandler;
        private readonly CRM.Application.CompanyIdentifiers.Services.ICompanyIdentifierValidationService _validationService;
        private readonly IAuditLogger _audit;

        public CompanyIdentifiersController(
            GetCompanyIdentifierValuesQueryHandler getHandler,
            SaveCompanyIdentifierValuesCommandHandler saveHandler,
            CRM.Application.CompanyIdentifiers.Services.ICompanyIdentifierValidationService validationService,
            IAuditLogger audit)
        {
            _getHandler = getHandler;
            _saveHandler = saveHandler;
            _validationService = validationService;
            _audit = audit;
        }

        [HttpGet("countries/{countryId}")]
        public async Task<IActionResult> GetByCountry([FromRoute] Guid countryId)
        {
            try
            {
                var query = new GetCompanyIdentifierValuesQuery
                {
                    CountryId = countryId
                };
                var result = await _getHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("company_identifiers_get_error", new { countryId, error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving identifier values." });
            }
        }

        [HttpPut("countries/{countryId}")]
        public async Task<IActionResult> Save([FromRoute] Guid countryId, [FromBody] SaveCompanyIdentifierValuesRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
                await _audit.LogAsync("company_identifiers_save_attempt", new { userId, countryId });

                // Ensure CountryId matches route
                request.CountryId = countryId;

                // Validate request
                var validator = new SaveCompanyIdentifierValuesRequestValidator(_validationService);
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { success = false, errors = validationResult.Errors });
                }

                var command = new SaveCompanyIdentifierValuesCommand
                {
                    Request = request,
                    UpdatedBy = userId
                };

                var result = await _saveHandler.Handle(command);
                await _audit.LogAsync("company_identifiers_save_success", new { userId, countryId });

                return Ok(new { success = true, message = "Identifier values saved successfully", data = result });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("company_identifiers_save_error", new { countryId, error = ex.Message });
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("company_identifiers_save_error", new { countryId, error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while saving identifier values." });
            }
        }
    }
}

