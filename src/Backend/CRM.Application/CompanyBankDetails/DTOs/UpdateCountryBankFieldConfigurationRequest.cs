namespace CRM.Application.CompanyBankDetails.DTOs
{
    public class UpdateCountryBankFieldConfigurationRequest
    {
        public bool IsRequired { get; set; }
        public string? ValidationRegex { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? DisplayName { get; set; }
        public string? HelpText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

