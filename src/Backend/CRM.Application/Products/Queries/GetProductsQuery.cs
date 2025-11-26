using System;
using CRM.Domain.Enums;

namespace CRM.Application.Products.Queries
{
    public class GetProductsQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public ProductType? ProductType { get; set; }
        public Guid? CategoryId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string? Currency { get; set; }
    }
}

