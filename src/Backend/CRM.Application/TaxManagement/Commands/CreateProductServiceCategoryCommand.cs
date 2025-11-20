using System;

namespace CRM.Application.TaxManagement.Commands
{
    public class CreateProductServiceCategoryCommand
    {
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryCode { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid CreatedByUserId { get; set; }
    }
}

