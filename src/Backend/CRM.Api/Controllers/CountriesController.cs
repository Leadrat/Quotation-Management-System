using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Results;
using CRM.Application.TaxManagement.Commands;
using CRM.Application.TaxManagement.Commands.Handlers;
using CRM.Application.TaxManagement.Queries;
using CRM.Application.TaxManagement.Queries.Handlers;
using CRM.Application.TaxManagement.Requests;
using CRM.Application.TaxManagement.Validators;
using CRM.Infrastructure.Logging;
using CRM.Infrastructure.Persistence;
using CRM.Api.Filters;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin/tax/countries")]
    [AdminOnly]
    public class CountriesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IAuditLogger _audit;
        private readonly IMapper _mapper;

        public CountriesController(AppDbContext db, IAuditLogger audit, IMapper mapper)
        {
            _db = db;
            _audit = audit;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? isActive, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var handler = new GetAllCountriesQueryHandler(_db, _mapper);
            var query = new GetAllCountriesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                IsActive = isActive
            };
            var result = await handler.Handle(query);

            return Ok(new
            {
                success = true,
                data = result.Data,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize,
                totalCount = result.TotalCount
            });
        }

        [HttpGet("{countryId}")]
        public async Task<IActionResult> GetById([FromRoute] Guid countryId)
        {
            try
            {
                var handler = new GetCountryByIdQueryHandler(_db, _mapper);
                var query = new GetCountryByIdQuery { CountryId = countryId };
                var result = await handler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCountryRequest body)
        {
            var validator = new CreateCountryRequestValidator();
            var result = validator.Validate(body);
            if (!result.IsValid)
            {
                return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
            }

            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
            if (!Guid.TryParse(sub, out var userId)) return Unauthorized();

            await _audit.LogAsync("country_create_attempt", new { userId, body.CountryName, body.CountryCode });

            var cmd = new CreateCountryCommand
            {
                CountryName = body.CountryName,
                CountryCode = body.CountryCode,
                TaxFrameworkType = body.TaxFrameworkType,
                DefaultCurrency = body.DefaultCurrency,
                IsActive = body.IsActive,
                IsDefault = body.IsDefault,
                CreatedByUserId = userId
            };

            try
            {
                var handler = new CreateCountryCommandHandler(_db, _mapper);
                var created = await handler.Handle(cmd);

                await _audit.LogAsync("country_create_success", new { userId, created.CountryId, created.CountryName });
                return StatusCode(201, new { success = true, message = "Country created successfully", data = created });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("country_create_error", new { userId, body.CountryName, body.CountryCode, error = ex.Message });
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [HttpPut("{countryId}")]
        public async Task<IActionResult> Update([FromRoute] Guid countryId, [FromBody] UpdateCountryRequest body)
        {
            var validator = new UpdateCountryRequestValidator();
            var result = validator.Validate(body);
            if (!result.IsValid)
            {
                return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
            }

            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
            if (!Guid.TryParse(sub, out var userId)) return Unauthorized();

            await _audit.LogAsync("country_update_attempt", new { userId, countryId });

            var cmd = new UpdateCountryCommand
            {
                CountryId = countryId,
                CountryName = body.CountryName,
                CountryCode = body.CountryCode,
                TaxFrameworkType = body.TaxFrameworkType,
                DefaultCurrency = body.DefaultCurrency,
                IsActive = body.IsActive,
                IsDefault = body.IsDefault,
                UpdatedByUserId = userId
            };

            try
            {
                var handler = new UpdateCountryCommandHandler(_db, _mapper);
                var updated = await handler.Handle(cmd);

                await _audit.LogAsync("country_update_success", new { userId, updated.CountryId });
                return Ok(new { success = true, message = "Country updated successfully", data = updated });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("country_update_error", new { userId, countryId, error = ex.Message });
                return NotFound(new { success = false, error = ex.Message });
            }
        }

        [HttpDelete("{countryId}")]
        public async Task<IActionResult> Delete([FromRoute] Guid countryId)
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
            if (!Guid.TryParse(sub, out var userId)) return Unauthorized();

            await _audit.LogAsync("country_delete_attempt", new { userId, countryId });

            var cmd = new DeleteCountryCommand
            {
                CountryId = countryId,
                DeletedByUserId = userId
            };

            try
            {
                var handler = new DeleteCountryCommandHandler(_db);
                await handler.Handle(cmd);

                await _audit.LogAsync("country_delete_success", new { userId, countryId });
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("country_delete_error", new { userId, countryId, error = ex.Message });
                return BadRequest(new { success = false, error = ex.Message });
            }
        }
    }
}

