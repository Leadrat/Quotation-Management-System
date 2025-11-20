namespace CRM.Application.CompanyIdentifiers.DTOs
{
    public class CreateIdentifierTypeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}

