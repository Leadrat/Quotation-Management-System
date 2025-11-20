using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using CRM.Application.TaxManagement.Dtos;
using CRM.Application.TaxManagement.Queries;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.TaxManagement.Queries.Handlers
{
    public class GetTaxCalculationLogQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetTaxCalculationLogQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<PagedResult<TaxCalculationLogDto>> Handle(GetTaxCalculationLogQuery q)
        {
            var pageNumber = q.PageNumber < 1 ? 1 : q.PageNumber;
            var pageSize = q.PageSize > 100 ? 100 : (q.PageSize < 1 ? 50 : q.PageSize);

            var query = _db.TaxCalculationLogs
                .AsNoTracking()
                .Include(log => log.Country)
                .Include(log => log.Jurisdiction)
                .Include(log => log.ChangedByUser)
                .AsQueryable();

            if (q.QuotationId.HasValue)
            {
                query = query.Where(log => log.QuotationId == q.QuotationId.Value);
            }

            if (q.CountryId.HasValue)
            {
                query = query.Where(log => log.CountryId == q.CountryId.Value);
            }

            if (q.JurisdictionId.HasValue)
            {
                query = query.Where(log => log.JurisdictionId == q.JurisdictionId.Value);
            }

            if (q.FromDate.HasValue)
            {
                query = query.Where(log => log.ChangedAt >= q.FromDate.Value);
            }

            if (q.ToDate.HasValue)
            {
                query = query.Where(log => log.ChangedAt <= q.ToDate.Value);
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(log => log.ChangedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync();

            var dtos = data.Select(log =>
            {
                var dto = _mapper.Map<TaxCalculationLogDto>(log);
                
                // Deserialize calculation details
                if (!string.IsNullOrWhiteSpace(log.CalculationDetails))
                {
                    try
                    {
                        dto.CalculationDetails = JsonSerializer.Deserialize<Dictionary<string, object>>(log.CalculationDetails) ?? new Dictionary<string, object>();
                    }
                    catch
                    {
                        dto.CalculationDetails = new Dictionary<string, object>();
                    }
                }

                // Set related data
                if (log.ChangedByUser != null)
                {
                    dto.ChangedByUserName = $"{log.ChangedByUser.FirstName} {log.ChangedByUser.LastName}";
                }
                if (log.Country != null)
                {
                    dto.CountryName = log.Country.CountryName;
                }
                if (log.Jurisdiction != null)
                {
                    dto.JurisdictionName = log.Jurisdiction.JurisdictionName;
                }

                return dto;
            }).ToArray();

            return new PagedResult<TaxCalculationLogDto>
            {
                Success = true,
                Data = dtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total
            };
        }
    }
}

