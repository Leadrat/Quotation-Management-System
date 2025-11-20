using CRM.Application.CompanyIdentifiers.DTOs;

namespace CRM.Application.CompanyIdentifiers.Commands
{
    public class SaveCompanyIdentifierValuesCommand
    {
        public SaveCompanyIdentifierValuesRequest Request { get; set; } = null!;
        public Guid UpdatedBy { get; set; }
    }
}

