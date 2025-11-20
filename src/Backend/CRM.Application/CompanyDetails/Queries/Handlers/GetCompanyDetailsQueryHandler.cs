using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyDetails.Dtos;
using CRM.Application.CompanyDetails.Queries;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.CompanyDetails.Queries.Handlers
{
    public class GetCompanyDetailsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetCompanyDetailsQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<CompanyDetailsDto> Handle(GetCompanyDetailsQuery query)
        {
            var companyDetails = await _db.CompanyDetails
                .Include(c => c.BankDetails)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (companyDetails == null)
            {
                // Return empty DTO if not configured yet
                return new CompanyDetailsDto
                {
                    CompanyDetailsId = Guid.Empty,
                    BankDetails = new List<BankDetailsDto>()
                };
            }

            return _mapper.Map<CompanyDetailsDto>(companyDetails);
        }
    }
}

