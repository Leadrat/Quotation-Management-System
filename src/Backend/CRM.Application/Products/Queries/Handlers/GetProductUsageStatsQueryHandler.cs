using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Products.DTOs;
using CRM.Application.Products.Queries;
using CRM.Application.Common.Persistence;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Products.Queries.Handlers
{
    public class GetProductUsageStatsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public GetProductUsageStatsQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ProductUsageStatsDto?> Handle(GetProductUsageStatsQuery q)
        {
            var product = await _db.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == q.ProductId);

            if (product == null)
            {
                return null;
            }

            // Get line items that use this product
            var lineItems = await _db.QuotationLineItems
                .AsNoTracking()
                .Where(li => li.ProductId == q.ProductId)
                .Include(li => li.Quotation)
                .ToListAsync();

            var quotations = lineItems
                .Select(li => li.QuotationId)
                .Distinct()
                .ToList();

            var quotationIds = quotations.ToList();
            var quotationsWithProduct = await _db.Quotations
                .AsNoTracking()
                .Where(qu => quotationIds.Contains(qu.QuotationId) && 
                            (qu.Status == QuotationStatus.Accepted || qu.Status == QuotationStatus.Sent))
                .ToListAsync();

            var totalRevenue = lineItems.Sum(li => li.Amount);
            var quotationCount = quotationsWithProduct.Count;

            return new ProductUsageStatsDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                TotalQuotationsUsedIn = quotationCount,
                TotalRevenueGenerated = totalRevenue,
                Currency = product.Currency
            };
        }
    }
}

