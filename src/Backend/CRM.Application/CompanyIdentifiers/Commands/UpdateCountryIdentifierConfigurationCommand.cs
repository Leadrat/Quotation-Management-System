using System;
using CRM.Application.CompanyIdentifiers.DTOs;

namespace CRM.Application.CompanyIdentifiers.Commands
{
    public class UpdateCountryIdentifierConfigurationCommand
    {
        public Guid ConfigurationId { get; set; }
        public UpdateCountryIdentifierConfigurationRequest Request { get; set; } = null!;
    }
}

