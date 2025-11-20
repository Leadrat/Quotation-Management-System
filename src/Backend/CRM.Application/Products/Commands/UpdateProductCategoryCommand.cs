using System;

namespace CRM.Application.Products.Commands
{
    public class UpdateProductCategoryCommand
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public bool IsActive { get; set; }
        public Guid UpdatedByUserId { get; set; }
    }
}

