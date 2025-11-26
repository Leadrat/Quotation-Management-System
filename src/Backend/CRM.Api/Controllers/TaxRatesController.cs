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
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin/tax/rates")]
    [AdminOnly]
    public class TaxRatesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IAuditLogger _audit;
        private readonly IMapper _mapper;

        public TaxRatesController(AppDbContext db, IAuditLogger audit, IMapper mapper)
        {
            _db = db;
            _audit = audit;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? jurisdictionId,
            [FromQuery] Guid? taxFrameworkId,
            [FromQuery] Guid? productServiceCategoryId,
            [FromQuery] DateOnly? asOfDate)
        {
            var handler = new GetAllTaxRatesQueryHandler(_db, _mapper);
            var query = new GetAllTaxRatesQuery
            {
                JurisdictionId = jurisdictionId,
                TaxFrameworkId = taxFrameworkId,
                ProductServiceCategoryId = productServiceCategoryId,
                AsOfDate = asOfDate
            };
            var result = await handler.Handle(query);

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        [HttpGet("jurisdiction/{jurisdictionId}")]
        public async Task<IActionResult> GetByJurisdiction(
            [FromRoute] Guid jurisdictionId,
            [FromQuery] Guid? productServiceCategoryId,
            [FromQuery] DateOnly? asOfDate)
        {
            var handler = new GetTaxRatesByJurisdictionQueryHandler(_db, _mapper);
            var query = new GetTaxRatesByJurisdictionQuery
            {
                JurisdictionId = jurisdictionId,
                ProductServiceCategoryId = productServiceCategoryId,
                AsOfDate = asOfDate
            };
            var result = await handler.Handle(query);

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaxRateRequest request)
        {
            var validator = new CreateTaxRateRequestValidator();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { success = false, errors = validationResult.Errors });
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());
            var handler = new CreateTaxRateCommandHandler(_db, _mapper);
            var command = new CreateTaxRateCommand
            {
                JurisdictionId = request.JurisdictionId,
                TaxFrameworkId = request.TaxFrameworkId,
                ProductServiceCategoryId = request.ProductServiceCategoryId,
                TaxRate = request.TaxRate,
                EffectiveFrom = request.EffectiveFrom,
                EffectiveTo = request.EffectiveTo,
                IsExempt = request.IsExempt,
                IsZeroRated = request.IsZeroRated,
                TaxComponents = request.TaxComponents.Select(c => new TaxComponentRateCommand
                {
                    Component = c.Component,
                    Rate = c.Rate
                }).ToList(),
                Description = request.Description,
                CreatedByUserId = userId
            };

            try
            {
                var result = await handler.Handle(command);
                await _audit.LogAsync("tax_rate_created", new { userId, result.TaxRateId, result.TaxRate });

                return CreatedAtAction(nameof(GetAll), new { }, new
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

        [HttpPut("{taxRateId}")]
        public async Task<IActionResult> Update([FromRoute] Guid taxRateId, [FromBody] UpdateTaxRateRequest request)
        {
            var validator = new UpdateTaxRateRequestValidator();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { success = false, errors = validationResult.Errors });
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());
            var handler = new UpdateTaxRateCommandHandler(_db, _mapper);
            var command = new UpdateTaxRateCommand
            {
                TaxRateId = taxRateId,
                JurisdictionId = request.JurisdictionId,
                TaxFrameworkId = request.TaxFrameworkId,
                ProductServiceCategoryId = request.ProductServiceCategoryId,
                TaxRate = request.TaxRate,
                EffectiveFrom = request.EffectiveFrom,
                EffectiveTo = request.EffectiveTo,
                IsExempt = request.IsExempt,
                IsZeroRated = request.IsZeroRated,
                TaxComponents = request.TaxComponents.Select(c => new TaxComponentRateCommand
                {
                    Component = c.Component,
                    Rate = c.Rate
                }).ToList(),
                Description = request.Description,
                UpdatedByUserId = userId
            };

            try
            {
                var result = await handler.Handle(command);
                await _audit.LogAsync("tax_rate_updated", new { userId, taxRateId, result.TaxRate });

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

        [HttpDelete("{taxRateId}")]
        public async Task<IActionResult> Delete([FromRoute] Guid taxRateId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());
            var handler = new DeleteTaxRateCommandHandler(_db);
            var command = new DeleteTaxRateCommand
            {
                TaxRateId = taxRateId,
                DeletedByUserId = userId
            };

            try
            {
                await handler.Handle(command);
                await _audit.LogAsync("tax_rate_deleted", new { userId, taxRateId });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}

