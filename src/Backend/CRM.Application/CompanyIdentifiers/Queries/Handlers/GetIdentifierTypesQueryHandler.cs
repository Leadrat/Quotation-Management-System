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
    public class GetIdentifierTypesQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetIdentifierTypesQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<IdentifierTypeDto>> Handle(GetIdentifierTypesQuery query)
        {
            var queryable = _db.IdentifierTypes
                .AsNoTracking()
                .Where(i => i.DeletedAt == null);

            if (!query.IncludeInactive)
            {
                queryable = queryable.Where(i => i.IsActive);
            }

            var entities = await queryable
                .OrderBy(i => i.DisplayName)
                .ToListAsync();

            return _mapper.Map<List<IdentifierTypeDto>>(entities);
        }
    }
}

