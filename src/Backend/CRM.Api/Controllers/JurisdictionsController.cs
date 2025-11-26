using System;
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
    [Route("api/v1/admin/tax/jurisdictions")]
    [AdminOnly]
    public class JurisdictionsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IAuditLogger _audit;
        private readonly IMapper _mapper;

        public JurisdictionsController(AppDbContext db, IAuditLogger audit, IMapper mapper)
        {
            _db = db;
            _audit = audit;
            _mapper = mapper;
        }

        [HttpGet("country/{countryId}")]
        public async Task<IActionResult> GetByCountry(
            [FromRoute] Guid countryId,
            [FromQuery] Guid? parentJurisdictionId,
            [FromQuery] bool? isActive)
        {
            var handler = new GetJurisdictionsByCountryQueryHandler(_db, _mapper);
            var query = new GetJurisdictionsByCountryQuery
            {
                CountryId = countryId,
                ParentJurisdictionId = parentJurisdictionId,
                IsActive = isActive
            };
            var result = await handler.Handle(query);

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        [HttpGet("{jurisdictionId}")]
        public async Task<IActionResult> GetById([FromRoute] Guid jurisdictionId)
        {
            var handler = new GetJurisdictionByIdQueryHandler(_db, _mapper);
            var query = new GetJurisdictionByIdQuery
            {
                JurisdictionId = jurisdictionId
            };
            var result = await handler.Handle(query);

            if (result == null)
            {
                return NotFound(new { success = false, message = "Jurisdiction not found" });
            }

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateJurisdictionRequest request)
        {
            var validator = new CreateJurisdictionRequestValidator();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { success = false, errors = validationResult.Errors });
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());
            var handler = new CreateJurisdictionCommandHandler(_db, _mapper);
            var command = new CreateJurisdictionCommand
            {
                CountryId = request.CountryId,
                ParentJurisdictionId = request.ParentJurisdictionId,
                JurisdictionName = request.JurisdictionName,
                JurisdictionCode = request.JurisdictionCode,
                JurisdictionType = request.JurisdictionType,
                IsActive = request.IsActive,
                CreatedByUserId = userId
            };

            try
            {
                var result = await handler.Handle(command);
                await _audit.LogAsync("jurisdiction_created", new { userId, result.JurisdictionId, result.JurisdictionName });

                return CreatedAtAction(nameof(GetById), new { jurisdictionId = result.JurisdictionId }, new
                {
                    success = true,
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{jurisdictionId}")]
        public async Task<IActionResult> Update([FromRoute] Guid jurisdictionId, [FromBody] UpdateJurisdictionRequest request)
        {
            var validator = new UpdateJurisdictionRequestValidator();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { success = false, errors = validationResult.Errors });
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());
            var handler = new UpdateJurisdictionCommandHandler(_db, _mapper);
            var command = new UpdateJurisdictionCommand
            {
                JurisdictionId = jurisdictionId,
                ParentJurisdictionId = request.ParentJurisdictionId,
                JurisdictionName = request.JurisdictionName,
                JurisdictionCode = request.JurisdictionCode,
                JurisdictionType = request.JurisdictionType,
                IsActive = request.IsActive,
                UpdatedByUserId = userId
            };

            try
            {
                var result = await handler.Handle(command);
                await _audit.LogAsync("jurisdiction_updated", new { userId, jurisdictionId, result.JurisdictionName });

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{jurisdictionId}")]
        public async Task<IActionResult> Delete([FromRoute] Guid jurisdictionId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());
            var handler = new DeleteJurisdictionCommandHandler(_db);
            var command = new DeleteJurisdictionCommand
            {
                JurisdictionId = jurisdictionId,
                DeletedByUserId = userId
            };

            try
            {
                await handler.Handle(command);
                await _audit.LogAsync("jurisdiction_deleted", new { userId, jurisdictionId });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}

