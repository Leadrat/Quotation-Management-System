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
    [Route("api/v1/admin/country-identifier-configurations")]
    [Authorize(Roles = "Admin")]
    public class CountryIdentifierConfigurationsController : ControllerBase
    {
        private readonly GetCountryIdentifierConfigurationsQueryHandler _getHandler;
        private readonly ConfigureCountryIdentifierCommandHandler _configureHandler;
        private readonly UpdateCountryIdentifierConfigurationCommandHandler _updateHandler;
        private readonly IAuditLogger _audit;

        public CountryIdentifierConfigurationsController(
            GetCountryIdentifierConfigurationsQueryHandler getHandler,
            ConfigureCountryIdentifierCommandHandler configureHandler,
            UpdateCountryIdentifierConfigurationCommandHandler updateHandler,
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
            [FromQuery] Guid? identifierTypeId,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = new GetCountryIdentifierConfigurationsQuery
                {
                    CountryId = countryId,
                    IdentifierTypeId = identifierTypeId,
                    IncludeInactive = includeInactive
                };
                var result = await _getHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("country_identifier_configurations_get_error", new { error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving configurations." });
            }
        }

        [HttpGet("countries/{countryId}")]
        public async Task<IActionResult> GetByCountry([FromRoute] Guid countryId, [FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = new GetCountryIdentifierConfigurationsQuery
                {
                    CountryId = countryId,
                    IncludeInactive = includeInactive
                };
                var result = await _getHandler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("country_identifier_configurations_get_by_country_error", new { countryId, error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving configurations." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Configure([FromBody] ConfigureCountryIdentifierRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
                await _audit.LogAsync("country_identifier_configure_attempt", new { userId, request.CountryId, request.IdentifierTypeId });

                // Validate request
                var validator = new ConfigureCountryIdentifierRequestValidator();
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { success = false, errors = validationResult.Errors });
                }

                var command = new ConfigureCountryIdentifierCommand
                {
                    Request = request
                };

                var result = await _configureHandler.Handle(command);
                await _audit.LogAsync("country_identifier_configure_success", new { userId, result.ConfigurationId });

                return StatusCode(201, new { success = true, message = "Configuration created successfully", data = result });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("country_identifier_configure_error", new { error = ex.Message });
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("country_identifier_configure_error", new { error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while creating configuration." });
            }
        }

        [HttpPut("{configurationId}")]
        public async Task<IActionResult> Update([FromRoute] Guid configurationId, [FromBody] UpdateCountryIdentifierConfigurationRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
                await _audit.LogAsync("country_identifier_configuration_update_attempt", new { userId, configurationId });

                // Validate request
                var validator = new UpdateCountryIdentifierConfigurationRequestValidator();
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { success = false, errors = validationResult.Errors });
                }

                var command = new UpdateCountryIdentifierConfigurationCommand
                {
                    ConfigurationId = configurationId,
                    Request = request
                };

                var result = await _updateHandler.Handle(command);
                await _audit.LogAsync("country_identifier_configuration_update_success", new { userId, configurationId });

                return Ok(new { success = true, message = "Configuration updated successfully", data = result });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("country_identifier_configuration_update_error", new { userId = User.FindFirstValue(ClaimTypes.NameIdentifier), configurationId, error = ex.Message });
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("country_identifier_configuration_update_error", new { userId = User.FindFirstValue(ClaimTypes.NameIdentifier), configurationId, error = ex.Message });
                return StatusCode(500, new { success = false, message = "An error occurred while updating configuration." });
            }
        }
    }
}

