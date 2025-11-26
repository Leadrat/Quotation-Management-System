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
    public class GetAllProductServiceCategoriesQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetAllProductServiceCategoriesQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ProductServiceCategoryDto[]> Handle(GetAllProductServiceCategoriesQuery q)
        {
            var query = _db.ProductServiceCategories.AsNoTracking();

            if (q.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == q.IsActive.Value);
            }

            var data = await query
                .OrderBy(c => c.CategoryName)
                .ProjectTo<ProductServiceCategoryDto>(_mapper.ConfigurationProvider)
                .ToArrayAsync();

            return data;
        }
    }
}

