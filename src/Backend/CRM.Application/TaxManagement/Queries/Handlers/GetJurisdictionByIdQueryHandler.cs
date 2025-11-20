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
    public class GetJurisdictionByIdQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetJurisdictionByIdQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<JurisdictionDto?> Handle(GetJurisdictionByIdQuery q)
        {
            var entity = await _db.Jurisdictions
                .AsNoTracking()
                .Include(j => j.Country)
                .Include(j => j.ParentJurisdiction)
                .FirstOrDefaultAsync(j => j.JurisdictionId == q.JurisdictionId && j.DeletedAt == null);

            if (entity == null)
            {
                return null;
            }

            var dto = _mapper.Map<JurisdictionDto>(entity);
            if (entity.Country != null)
            {
                dto.CountryName = entity.Country.CountryName;
            }
            if (entity.ParentJurisdiction != null)
            {
                dto.ParentJurisdictionName = entity.ParentJurisdiction.JurisdictionName;
            }

            return dto;
        }
    }
}

