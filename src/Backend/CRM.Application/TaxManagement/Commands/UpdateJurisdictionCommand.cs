using System;

namespace CRM.Application.TaxManagement.Commands
{
    public class UpdateJurisdictionCommand
    {
        public Guid JurisdictionId { get; set; }
        public Guid? ParentJurisdictionId { get; set; }
        public string JurisdictionName { get; set; } = string.Empty;
        public string? JurisdictionCode { get; set; }
        public string? JurisdictionType { get; set; }
        public bool IsActive { get; set; }
        public Guid UpdatedByUserId { get; set; }
    }
}

