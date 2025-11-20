using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Application.TaxManagement.Queries;
using CRM.Application.TaxManagement.Queries.Handlers;
using CRM.Infrastructure.Persistence;
using CRM.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin/tax/audit-log")]
    [AdminOnly]
    public class TaxAuditLogController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly GetTaxCalculationLogQueryHandler _queryHandler;

        public TaxAuditLogController(AppDbContext db, GetTaxCalculationLogQueryHandler queryHandler)
        {
            _db = db;
            _queryHandler = queryHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLog(
            [FromQuery] Guid? quotationId,
            [FromQuery] Guid? countryId,
            [FromQuery] Guid? jurisdictionId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var query = new GetTaxCalculationLogQuery
            {
                QuotationId = quotationId,
                CountryId = countryId,
                JurisdictionId = jurisdictionId,
                FromDate = fromDate,
                ToDate = toDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _queryHandler.Handle(query);

            return Ok(new
            {
                success = true,
                data = result.Data,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize,
                totalCount = result.TotalCount
            });
        }
    }
}

