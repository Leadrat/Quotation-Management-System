using System;

namespace CRM.Application.TaxManagement.Dtos
{
    public class ProductServiceCategoryDto
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryCode { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}

