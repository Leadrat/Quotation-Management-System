using System;
using CRM.Application.CompanyBankDetails.DTOs;

namespace CRM.Application.CompanyBankDetails.Commands
{
    public class UpdateCountryBankFieldConfigurationCommand
    {
        public Guid ConfigurationId { get; set; }
        public UpdateCountryBankFieldConfigurationRequest Request { get; set; } = null!;
    }
}

