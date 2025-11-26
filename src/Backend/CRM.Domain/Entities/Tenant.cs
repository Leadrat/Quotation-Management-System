using System;

namespace CRM.Domain.Entities
{
    public class Tenant
    {
        public Guid Id { get; set; }

        // Human-readable / business identifier set by SuperAdmin (e.g. "infosys", "tcs-mumbai")
        public string TenantId { get; set; } = default!;

        public string Name { get; set; } = default!;

        public bool IsActive { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
