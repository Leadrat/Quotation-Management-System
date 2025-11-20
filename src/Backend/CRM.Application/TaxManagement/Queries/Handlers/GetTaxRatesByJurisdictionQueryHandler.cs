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
    public class GetTaxRatesByJurisdictionQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetTaxRatesByJurisdictionQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<TaxRateDto[]> Handle(GetTaxRatesByJurisdictionQuery q)
        {
            var asOfDate = q.AsOfDate ?? DateOnly.FromDateTime(DateTime.Today);

            var query = _db.TaxRates
                .AsNoTracking()
                .Include(tr => tr.Jurisdiction)
                .Include(tr => tr.ProductServiceCategory)
                .Include(tr => tr.TaxFramework)
                .Where(tr =>
                    tr.JurisdictionId == q.JurisdictionId &&
                    tr.EffectiveFrom <= asOfDate &&
                    (tr.EffectiveTo == null || tr.EffectiveTo >= asOfDate));

            if (q.ProductServiceCategoryId.HasValue)
            {
                query = query.Where(tr => tr.ProductServiceCategoryId == q.ProductServiceCategoryId.Value);
            }

            var data = await query
                .OrderByDescending(tr => tr.EffectiveFrom)
                .ThenBy(tr => tr.ProductServiceCategoryId)
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

