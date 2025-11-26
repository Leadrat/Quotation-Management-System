using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CRM.Application.Products.DTOs;
using CRM.Application.Products.Queries;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Products.Queries.Handlers
{
    public class GetProductCategoriesQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetProductCategoriesQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ProductCategoryDto[]> Handle(GetProductCategoriesQuery q)
        {
            var query = _db.ProductCategories.AsNoTracking();

            if (q.ParentCategoryId.HasValue)
            {
                query = query.Where(c => c.ParentCategoryId == q.ParentCategoryId.Value);
            }
            else
            {
                // If no parent specified, include all (or filter by active)
                if (q.IsActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == q.IsActive.Value);
                }
            }

            return await query
                .OrderBy(c => c.CategoryName)
                .Include(c => c.ParentCategory)
                .ProjectTo<ProductCategoryDto>(_mapper.ConfigurationProvider)
                .ToArrayAsync();
        }
    }
}

