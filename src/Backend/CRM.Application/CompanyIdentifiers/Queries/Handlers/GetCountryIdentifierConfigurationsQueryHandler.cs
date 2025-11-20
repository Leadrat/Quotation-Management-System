using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyIdentifiers.DTOs;
using CRM.Application.CompanyIdentifiers.Queries;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.CompanyIdentifiers.Queries.Handlers
{
    public class GetCountryIdentifierConfigurationsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetCountryIdentifierConfigurationsQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<CountryIdentifierConfigurationDto>> Handle(GetCountryIdentifierConfigurationsQuery query)
        {
            var queryable = _db.CountryIdentifierConfigurations
                .AsNoTracking()
                .Include(c => c.Country)
                .Include(c => c.IdentifierType)
                .Where(c => c.DeletedAt == null);

            if (query.CountryId.HasValue)
            {
                queryable = queryable.Where(c => c.CountryId == query.CountryId.Value);
            }

            if (query.IdentifierTypeId.HasValue)
            {
                queryable = queryable.Where(c => c.IdentifierTypeId == query.IdentifierTypeId.Value);
            }

            if (!query.IncludeInactive)
            {
                queryable = queryable.Where(c => c.IsActive);
            }

            var entities = await queryable
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.DisplayName ?? c.IdentifierType!.DisplayName)
                .ToListAsync();

            return _mapper.Map<List<CountryIdentifierConfigurationDto>>(entities);
        }
    }
}

