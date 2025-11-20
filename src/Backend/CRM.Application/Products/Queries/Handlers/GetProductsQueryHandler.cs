using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CRM.Application.Products.DTOs;
using CRM.Application.Products.Queries;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Products.Queries.Handlers
{
    public class GetProductsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetProductsQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery q)
        {
            var pageNumber = q.PageNumber < 1 ? 1 : q.PageNumber;
            var pageSize = q.PageSize > 100 ? 100 : (q.PageSize < 1 ? 10 : q.PageSize);

            var query = _db.Products.AsNoTracking();

            // Apply filters
            if (q.ProductType.HasValue)
            {
                query = query.Where(p => p.ProductType == q.ProductType.Value);
            }

            if (q.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == q.CategoryId.Value);
            }

            if (q.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == q.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var searchLower = q.Search.ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(searchLower) ||
                                        (p.Description != null && p.Description.ToLower().Contains(searchLower)));
            }

            if (!string.IsNullOrWhiteSpace(q.Currency))
            {
                query = query.Where(p => p.Currency == q.Currency);
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Category)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .ToArrayAsync();

            return new PagedResult<ProductDto>
            {
                Success = true,
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total
            };
        }
    }
}

