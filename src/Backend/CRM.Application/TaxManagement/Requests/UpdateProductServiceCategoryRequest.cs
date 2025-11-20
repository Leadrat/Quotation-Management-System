namespace CRM.Application.TaxManagement.Requests
{
    public class UpdateProductServiceCategoryRequest
    {
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryCode { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}

