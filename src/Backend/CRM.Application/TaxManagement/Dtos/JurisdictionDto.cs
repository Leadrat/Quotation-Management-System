using System;

namespace CRM.Application.TaxManagement.Dtos
{
    public class JurisdictionDto
    {
        public Guid JurisdictionId { get; set; }
        public Guid CountryId { get; set; }
        public Guid? ParentJurisdictionId { get; set; }
        public string JurisdictionName { get; set; } = string.Empty;
        public string? JurisdictionCode { get; set; }
        public string? JurisdictionType { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string? CountryName { get; set; }
        public string? ParentJurisdictionName { get; set; }
    }
}

