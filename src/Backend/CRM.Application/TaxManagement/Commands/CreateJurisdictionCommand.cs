using System;

namespace CRM.Application.TaxManagement.Commands
{
    public class CreateJurisdictionCommand
    {
        public Guid CountryId { get; set; }
        public Guid? ParentJurisdictionId { get; set; }
        public string JurisdictionName { get; set; } = string.Empty;
        public string? JurisdictionCode { get; set; }
        public string? JurisdictionType { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid CreatedByUserId { get; set; }
    }
}

