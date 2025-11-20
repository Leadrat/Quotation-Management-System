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
    public class GetTaxFrameworkByIdQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetTaxFrameworkByIdQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<TaxFrameworkDto> Handle(GetTaxFrameworkByIdQuery q)
        {
            var entity = await _db.TaxFrameworks
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.TaxFrameworkId == q.TaxFrameworkId && f.DeletedAt == null);
            
            if (entity == null)
            {
                throw new InvalidOperationException($"Tax framework with ID '{q.TaxFrameworkId}' not found");
            }

            return _mapper.Map<TaxFrameworkDto>(entity);
        }
    }
}

