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
    public class GetBankFieldTypesQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetBankFieldTypesQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<BankFieldTypeDto>> Handle(GetBankFieldTypesQuery query)
        {
            var queryable = _db.BankFieldTypes
                .AsNoTracking()
                .Where(b => b.DeletedAt == null);

            if (!query.IncludeInactive)
            {
                queryable = queryable.Where(b => b.IsActive);
            }

            var entities = await queryable
                .OrderBy(b => b.DisplayName)
                .ToListAsync();

            return _mapper.Map<List<BankFieldTypeDto>>(entities);
        }
    }
}

