using System;

namespace CRM.Application.Products.Requests
{
    public class CreateProductCategoryRequest
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

