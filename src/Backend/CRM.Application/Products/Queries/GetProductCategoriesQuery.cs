using System;

namespace CRM.Application.Products.Queries
{
    public class GetProductCategoriesQuery
    {
        public Guid? ParentCategoryId { get; set; }
        public bool? IsActive { get; set; }
    }
}

