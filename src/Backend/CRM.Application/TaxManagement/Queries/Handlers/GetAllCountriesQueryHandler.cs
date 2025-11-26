using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using CRM.Application.TaxManagement.Dtos;
using CRM.Application.TaxManagement.Queries;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.TaxManagement.Queries.Handlers
{
    public class GetAllCountriesQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetAllCountriesQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<PagedResult<CountryDto>> Handle(GetAllCountriesQuery q)
        {
            var pageNumber = q.PageNumber < 1 ? 1 : q.PageNumber;
            // Allow larger pageSize for company details dropdown (up to 1000)
            var pageSize = q.PageSize > 1000 ? 1000 : (q.PageSize < 1 ? 10 : q.PageSize);

            var query = _db.Countries.AsNoTracking();

            if (q.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == q.IsActive.Value);
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<CountryDto>(_mapper.ConfigurationProvider)
                .ToArrayAsync();

            return new PagedResult<CountryDto>
            {
                Success = true,
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total
            };
        }
    }
}

