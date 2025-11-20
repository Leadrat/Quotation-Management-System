using System;

namespace CRM.Application.CompanyIdentifiers.DTOs
{
    public class UpdateIdentifierTypeRequest
    {
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

