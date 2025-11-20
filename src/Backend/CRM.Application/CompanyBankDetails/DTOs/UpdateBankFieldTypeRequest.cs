namespace CRM.Application.CompanyBankDetails.DTOs
{
    public class UpdateBankFieldTypeRequest
    {
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

