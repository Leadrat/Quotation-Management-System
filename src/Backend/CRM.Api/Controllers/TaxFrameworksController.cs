using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
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
    [Route("api/v1/admin/tax/frameworks")]
    [AdminOnly]
    public class TaxFrameworksController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IAuditLogger _audit;
        private readonly IMapper _mapper;

        public TaxFrameworksController(AppDbContext db, IAuditLogger audit, IMapper mapper)
        {
            _db = db;
            _audit = audit;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? countryId, [FromQuery] bool? isActive)
        {
            var handler = new GetAllTaxFrameworksQueryHandler(_db, _mapper);
            var query = new GetAllTaxFrameworksQuery
            {
                CountryId = countryId,
                IsActive = isActive
            };
            var result = await handler.Handle(query);

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        [HttpGet("{frameworkId}")]
        public async Task<IActionResult> GetById([FromRoute] Guid frameworkId)
        {
            try
            {
                var handler = new GetTaxFrameworkByIdQueryHandler(_db, _mapper);
                var query = new GetTaxFrameworkByIdQuery { TaxFrameworkId = frameworkId };
                var result = await handler.Handle(query);
                return Ok(new { success = true, data = result });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaxFrameworkRequest body)
        {
            var validator = new CreateTaxFrameworkRequestValidator();
            var result = validator.Validate(body);
            if (!result.IsValid)
            {
                return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
            }

            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
            if (!Guid.TryParse(sub, out var userId)) return Unauthorized();

            await _audit.LogAsync("tax_framework_create_attempt", new { userId, body.CountryId, body.FrameworkName });

            var cmd = new CreateTaxFrameworkCommand
            {
                CountryId = body.CountryId,
                FrameworkName = body.FrameworkName,
                FrameworkType = body.FrameworkType,
                Description = body.Description,
                TaxComponents = body.TaxComponents,
                CreatedByUserId = userId
            };

            try
            {
                var handler = new CreateTaxFrameworkCommandHandler(_db, _mapper);
                var created = await handler.Handle(cmd);

                await _audit.LogAsync("tax_framework_create_success", new { userId, created.TaxFrameworkId, created.FrameworkName });
                return StatusCode(201, new { success = true, message = "Tax framework created successfully", data = created });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("tax_framework_create_error", new { userId, body.CountryId, body.FrameworkName, error = ex.Message });
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [HttpPut("{frameworkId}")]
        public async Task<IActionResult> Update([FromRoute] Guid frameworkId, [FromBody] UpdateTaxFrameworkRequest body)
        {
            var validator = new UpdateTaxFrameworkRequestValidator();
            var result = validator.Validate(body);
            if (!result.IsValid)
            {
                return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
            }

            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
            if (!Guid.TryParse(sub, out var userId)) return Unauthorized();

            await _audit.LogAsync("tax_framework_update_attempt", new { userId, frameworkId });

            var cmd = new UpdateTaxFrameworkCommand
            {
                TaxFrameworkId = frameworkId,
                FrameworkName = body.FrameworkName,
                FrameworkType = body.FrameworkType,
                Description = body.Description,
                TaxComponents = body.TaxComponents,
                IsActive = body.IsActive,
                UpdatedByUserId = userId
            };

            try
            {
                var handler = new UpdateTaxFrameworkCommandHandler(_db, _mapper);
                var updated = await handler.Handle(cmd);

                await _audit.LogAsync("tax_framework_update_success", new { userId, updated.TaxFrameworkId });
                return Ok(new { success = true, message = "Tax framework updated successfully", data = updated });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("tax_framework_update_error", new { userId, frameworkId, error = ex.Message });
                return NotFound(new { success = false, error = ex.Message });
            }
        }
    }
}

