using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.TaxManagement.Dtos;
using CRM.Application.TaxManagement.Queries;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.TaxManagement.Queries.Handlers
{
    public class GetCountryByIdQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetCountryByIdQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<CountryDto> Handle(GetCountryByIdQuery q)
        {
            var entity = await _db.Countries
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CountryId == q.CountryId && c.DeletedAt == null);
            
            if (entity == null)
            {
                throw new InvalidOperationException($"Country with ID '{q.CountryId}' not found");
            }

            return _mapper.Map<CountryDto>(entity);
        }
    }
}

