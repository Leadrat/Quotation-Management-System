namespace CRM.Application.CompanyBankDetails.DTOs
{
    public class CreateBankFieldTypeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}

