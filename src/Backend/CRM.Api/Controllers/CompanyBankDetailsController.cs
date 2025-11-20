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
    [Route("api/v1/company-details/bank-details")]
    [Authorize(Roles = "Admin")]
    public class CompanyBankDetailsController : ControllerBase
    {
        private readonly GetCompanyBankDetailsQueryHandler _getHandler;
        private readonly SaveCompanyBankDetailsCommandHandler _saveHandler;
        private readonly CRM.Application.CompanyBankDetails.Services.ICompanyBankDetailsValidationService _validationService;
        private readonly IAuditLogger _audit;

        public CompanyBankDetailsController(
            GetCompanyBankDetailsQueryHandler getHandler,
            SaveCompanyBankDetailsCommandHandler saveHandler,
            CRM.Application.CompanyBankDetails.Services.ICompanyBankDetailsValidationService validationService,
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
                var query = new GetCompanyBankDetailsQuery
                {
                    CountryId = countryId
                };
                var result = await _getHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("company_bank_details_get_error", new { countryId, error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving bank details." });
            }
        }

        [HttpPut("countries/{countryId}")]
        public async Task<IActionResult> Save([FromRoute] Guid countryId, [FromBody] SaveCompanyBankDetailsRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
                await _audit.LogAsync("company_bank_details_save_attempt", new { userId, countryId });

                // Ensure CountryId matches route
                request.CountryId = countryId;

                // Validate request
                var validator = new SaveCompanyBankDetailsRequestValidator(_validationService);
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { success = false, errors = validationResult.Errors });
                }

                var command = new SaveCompanyBankDetailsCommand
                {
                    Request = request,
                    UpdatedBy = userId
                };

                var result = await _saveHandler.Handle(command);
                await _audit.LogAsync("company_bank_details_save_success", new { userId, countryId });

                return Ok(new { success = true, message = "Bank details saved successfully", data = result });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("company_bank_details_save_error", new { countryId, error = ex.Message });
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("company_bank_details_save_error", new { countryId, error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while saving bank details." });
            }
        }
    }
}

