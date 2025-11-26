using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CRM.Domain.Entities
{
    [Table("Clients")]
    public class Client
    {
        public Guid ClientId { get; set; }
        public Guid TenantId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? PhoneCode { get; set; }
        public string? Gstin { get; set; }
        public string? StateCode { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PinCode { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? JurisdictionId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public virtual Tenant? Tenant { get; set; }
        public virtual User CreatedByUser { get; set; } = null!;

        public bool IsActive() => DeletedAt == null;

        public string GetFullAddress()
        {
            var parts = new[] { Address, City, State, PinCode };
            return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        public string GetDisplayName()
        {
            return string.IsNullOrWhiteSpace(ContactName)
                ? CompanyName
                : $"{CompanyName} ({ContactName})";
        }
    }
}
