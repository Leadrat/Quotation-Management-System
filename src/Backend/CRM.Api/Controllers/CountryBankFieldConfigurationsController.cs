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
    [Route("api/v1/admin/country-bank-field-configurations")]
    [Authorize(Roles = "Admin")]
    public class CountryBankFieldConfigurationsController : ControllerBase
    {
        private readonly GetCountryBankFieldConfigurationsQueryHandler _getHandler;
        private readonly ConfigureCountryBankFieldCommandHandler _configureHandler;
        private readonly UpdateCountryBankFieldConfigurationCommandHandler _updateHandler;
        private readonly IAuditLogger _audit;

        public CountryBankFieldConfigurationsController(
            GetCountryBankFieldConfigurationsQueryHandler getHandler,
            ConfigureCountryBankFieldCommandHandler configureHandler,
            UpdateCountryBankFieldConfigurationCommandHandler updateHandler,
            IAuditLogger audit)
        {
            _getHandler = getHandler;
            _configureHandler = configureHandler;
            _updateHandler = updateHandler;
            _audit = audit;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? countryId,
            [FromQuery] Guid? bankFieldTypeId,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = new GetCountryBankFieldConfigurationsQuery
                {
                    CountryId = countryId,
                    BankFieldTypeId = bankFieldTypeId,
                    IncludeInactive = includeInactive
                };
                var result = await _getHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("country_bank_field_configurations_get_error", new { error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving configurations." });
            }
        }

        [HttpGet("countries/{countryId}")]
        public async Task<IActionResult> GetByCountry([FromRoute] Guid countryId, [FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = new GetCountryBankFieldConfigurationsQuery
                {
                    CountryId = countryId,
                    IncludeInactive = includeInactive
                };
                var result = await _getHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("country_bank_field_configurations_get_by_country_error", new { countryId, error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving configurations." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Configure([FromBody] ConfigureCountryBankFieldRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
                await _audit.LogAsync("country_bank_field_configure_attempt", new { userId, request.CountryId, request.BankFieldTypeId });

                // Validate request
                var validator = new ConfigureCountryBankFieldRequestValidator();
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { success = false, errors = validationResult.Errors });
                }

                var command = new ConfigureCountryBankFieldCommand
                {
                    Request = request
                };

                var result = await _configureHandler.Handle(command);
                await _audit.LogAsync("country_bank_field_configure_success", new { userId, result.ConfigurationId });

                return StatusCode(201, new { success = true, message = "Configuration created successfully", data = result });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("country_bank_field_configure_error", new { error = ex.Message });
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("country_bank_field_configure_error", new { error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while creating configuration." });
            }
        }

        [HttpPut("{configurationId}")]
        public async Task<IActionResult> Update([FromRoute] Guid configurationId, [FromBody] UpdateCountryBankFieldConfigurationRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
                await _audit.LogAsync("country_bank_field_configuration_update_attempt", new { userId, configurationId });

                // Validate request
                var validator = new UpdateCountryBankFieldConfigurationRequestValidator();
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { success = false, errors = validationResult.Errors });
                }

                var command = new UpdateCountryBankFieldConfigurationCommand
                {
                    ConfigurationId = configurationId,
                    Request = request
                };

                var result = await _updateHandler.Handle(command);
                await _audit.LogAsync("country_bank_field_configuration_update_success", new { userId, configurationId });

                return Ok(new { success = true, message = "Configuration updated successfully", data = result });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("country_bank_field_configuration_update_error", new { userId = User.FindFirstValue(ClaimTypes.NameIdentifier), configurationId, error = ex.Message });
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("country_bank_field_configuration_update_error", new { userId = User.FindFirstValue(ClaimTypes.NameIdentifier), configurationId, error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while updating configuration." });
            }
        }
    }
}

