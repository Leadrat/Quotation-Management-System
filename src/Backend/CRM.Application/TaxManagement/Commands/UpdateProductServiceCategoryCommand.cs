using System;

namespace CRM.Application.TaxManagement.Commands
{
    public class UpdateProductServiceCategoryCommand
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryCode { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public Guid UpdatedByUserId { get; set; }
    }
}

