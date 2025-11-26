using System;

namespace CRM.Application.Products.Commands
{
    public class CreateProductCategoryCommand
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid CreatedByUserId { get; set; }
    }
}

