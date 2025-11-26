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
    public class GetJurisdictionsByCountryQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetJurisdictionsByCountryQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<JurisdictionDto[]> Handle(GetJurisdictionsByCountryQuery q)
        {
            var query = _db.Jurisdictions
                .AsNoTracking()
                .Include(j => j.Country)
                .Include(j => j.ParentJurisdiction)
                .Where(j => j.CountryId == q.CountryId);

            if (q.ParentJurisdictionId.HasValue)
            {
                query = query.Where(j => j.ParentJurisdictionId == q.ParentJurisdictionId.Value);
            }
            else
            {
                query = query.Where(j => j.ParentJurisdictionId == null);
            }

            if (q.IsActive.HasValue)
            {
                query = query.Where(j => j.IsActive == q.IsActive.Value);
            }

            var data = await query
                .OrderBy(j => j.JurisdictionName)
                .ProjectTo<JurisdictionDto>(_mapper.ConfigurationProvider)
                .ToArrayAsync();

            // Map related data
            foreach (var dto in data)
            {
                var jurisdiction = await _db.Jurisdictions
                    .Include(j => j.Country)
                    .Include(j => j.ParentJurisdiction)
                    .FirstOrDefaultAsync(j => j.JurisdictionId == dto.JurisdictionId);

                if (jurisdiction?.Country != null)
                {
                    dto.CountryName = jurisdiction.Country.CountryName;
                }
                if (jurisdiction?.ParentJurisdiction != null)
                {
                    dto.ParentJurisdictionName = jurisdiction.ParentJurisdiction.JurisdictionName;
                }
            }

            return data;
        }
    }
}

