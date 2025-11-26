using System;
using CRM.Application.CompanyIdentifiers.DTOs;

namespace CRM.Application.CompanyIdentifiers.Commands
{
    public class UpdateIdentifierTypeCommand
    {
        public Guid IdentifierTypeId { get; set; }
        public UpdateIdentifierTypeRequest Request { get; set; } = null!;
    }
}

