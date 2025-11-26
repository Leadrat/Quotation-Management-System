using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyBankDetails.DTOs;
using CRM.Application.CompanyBankDetails.Queries;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.CompanyBankDetails.Queries.Handlers
{
    public class GetCountryBankFieldConfigurationsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetCountryBankFieldConfigurationsQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<CountryBankFieldConfigurationDto>> Handle(GetCountryBankFieldConfigurationsQuery query)
        {
            var queryable = _db.CountryBankFieldConfigurations
                .AsNoTracking()
                .Include(c => c.Country)
                .Include(c => c.BankFieldType)
                .Where(c => c.DeletedAt == null);

            if (query.CountryId.HasValue)
            {
                queryable = queryable.Where(c => c.CountryId == query.CountryId.Value);
            }

            if (query.BankFieldTypeId.HasValue)
            {
                queryable = queryable.Where(c => c.BankFieldTypeId == query.BankFieldTypeId.Value);
            }

            if (!query.IncludeInactive)
            {
                queryable = queryable.Where(c => c.IsActive);
            }

            var entities = await queryable
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.DisplayName ?? c.BankFieldType!.DisplayName)
                .ToListAsync();

            return _mapper.Map<List<CountryBankFieldConfigurationDto>>(entities);
        }
    }
}

