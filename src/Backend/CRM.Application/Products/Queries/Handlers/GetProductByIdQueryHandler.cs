using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Products.DTOs;
using CRM.Application.Products.Queries;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Products.Queries.Handlers
{
    public class GetProductByIdQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetProductByIdQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ProductDto?> Handle(GetProductByIdQuery q)
        {
            var product = await _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == q.ProductId);

            if (product == null)
            {
                return null;
            }

            return _mapper.Map<ProductDto>(product);
        }
    }
}

