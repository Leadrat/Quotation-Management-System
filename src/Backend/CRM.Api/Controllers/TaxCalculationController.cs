using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Application.TaxManagement.Queries;
using CRM.Application.TaxManagement.Queries.Handlers;
using CRM.Application.TaxManagement.Requests;
using CRM.Application.TaxManagement.Validators;
using CRM.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/tax/calculation")]
    [Authorize]
    public class TaxCalculationController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly PreviewTaxCalculationQueryHandler _previewHandler;

        public TaxCalculationController(AppDbContext db, PreviewTaxCalculationQueryHandler previewHandler)
        {
            _db = db;
            _previewHandler = previewHandler;
        }

        [HttpGet("countries")]
        [Authorize(Roles = "SalesRep,Admin")]
        public async Task<IActionResult> GetActiveCountries()
        {
            var countries = await _db.Countries
                .Where(c => c.IsActive && c.DeletedAt == null)
                .OrderBy(c => c.CountryName)
                .Select(c => new
                {
                    countryId = c.CountryId,
                    countryName = c.CountryName,
                    countryCode = c.CountryCode,
                    isDefault = c.IsDefault
                })
                .ToListAsync();

            return Ok(new { success = true, data = countries });
        }

        [HttpPost("preview")]
        public async Task<IActionResult> PreviewTaxCalculation([FromBody] PreviewTaxCalculationRequest request)
        {
            var validator = new PreviewTaxCalculationRequestValidator();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { success = false, errors = validationResult.Errors });
            }

            var query = new PreviewTaxCalculationQuery
            {
                ClientId = request.ClientId,
                LineItems = request.LineItems.Select(li => new CRM.Application.TaxManagement.Services.LineItemTaxInput
                {
                    LineItemId = li.LineItemId,
                    ProductServiceCategoryId = li.ProductServiceCategoryId,
                    Amount = li.Amount
                }).ToList(),
                Subtotal = request.Subtotal,
                DiscountAmount = request.DiscountAmount,
                CalculationDate = request.CalculationDate ?? DateTime.UtcNow,
                CountryId = request.CountryId
            };

            try
            {
                var result = await _previewHandler.Handle(query);
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
    }
}

