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
    public class GetProductServiceCategoryByIdQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetProductServiceCategoryByIdQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ProductServiceCategoryDto?> Handle(GetProductServiceCategoryByIdQuery q)
        {
            var entity = await _db.ProductServiceCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == q.CategoryId && c.DeletedAt == null);

            if (entity == null)
            {
                return null;
            }

            return _mapper.Map<ProductServiceCategoryDto>(entity);
        }
    }
}

