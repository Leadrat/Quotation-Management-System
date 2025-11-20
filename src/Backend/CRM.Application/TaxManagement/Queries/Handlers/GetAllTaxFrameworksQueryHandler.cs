using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CRM.Application.Common.Persistence;
using CRM.Application.TaxManagement.Dtos;
using CRM.Application.TaxManagement.Queries;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.TaxManagement.Queries.Handlers
{
    public class GetAllTaxFrameworksQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetAllTaxFrameworksQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<TaxFrameworkDto[]> Handle(GetAllTaxFrameworksQuery q)
        {
            var query = _db.TaxFrameworks.AsNoTracking();

            if (q.CountryId.HasValue)
            {
                query = query.Where(f => f.CountryId == q.CountryId.Value);
            }

            if (q.IsActive.HasValue)
            {
                query = query.Where(f => f.IsActive == q.IsActive.Value);
            }

            var data = await query
                .OrderByDescending(f => f.CreatedAt)
                .ProjectTo<TaxFrameworkDto>(_mapper.ConfigurationProvider)
                .ToArrayAsync();

            return data;
        }
    }
}

