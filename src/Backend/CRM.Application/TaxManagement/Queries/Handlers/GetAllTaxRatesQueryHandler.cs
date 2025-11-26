using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.TaxManagement.Dtos;
using CRM.Application.TaxManagement.Queries;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.TaxManagement.Queries.Handlers
{
    public class GetAllTaxRatesQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetAllTaxRatesQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<TaxRateDto[]> Handle(GetAllTaxRatesQuery q)
        {
            var query = _db.TaxRates
                .AsNoTracking()
                .Include(tr => tr.Jurisdiction)
                .Include(tr => tr.ProductServiceCategory)
                .Include(tr => tr.TaxFramework)
                .AsQueryable();

            if (q.JurisdictionId.HasValue)
            {
                query = query.Where(tr => tr.JurisdictionId == q.JurisdictionId.Value);
            }

            if (q.TaxFrameworkId.HasValue)
            {
                query = query.Where(tr => tr.TaxFrameworkId == q.TaxFrameworkId.Value);
            }

            if (q.ProductServiceCategoryId.HasValue)
            {
                query = query.Where(tr => tr.ProductServiceCategoryId == q.ProductServiceCategoryId.Value);
            }

            if (q.AsOfDate.HasValue)
            {
                var asOfDate = q.AsOfDate.Value;
                query = query.Where(tr =>
                    tr.EffectiveFrom <= asOfDate &&
                    (tr.EffectiveTo == null || tr.EffectiveTo >= asOfDate));
            }

            var data = await query
                .OrderByDescending(tr => tr.EffectiveFrom)
                .ThenBy(tr => tr.JurisdictionId)
                .ToArrayAsync();

            var dtos = data.Select(tr =>
            {
                var dto = _mapper.Map<TaxRateDto>(tr);
                dto.TaxComponents = tr.GetTaxComponentRates().Select(c => new TaxComponentRateDto
                {
                    Component = c.Component,
                    Rate = c.Rate
                }).ToList();
                if (tr.Jurisdiction != null)
                {
                    dto.JurisdictionName = tr.Jurisdiction.JurisdictionName;
                }
                if (tr.ProductServiceCategory != null)
                {
                    dto.CategoryName = tr.ProductServiceCategory.CategoryName;
                }
                if (tr.TaxFramework != null)
                {
                    dto.FrameworkName = tr.TaxFramework.FrameworkName;
                }
                return dto;
            }).ToArray();

            return dtos;
        }
    }
}

