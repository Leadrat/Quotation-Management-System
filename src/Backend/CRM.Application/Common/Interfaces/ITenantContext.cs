using System;

namespace CRM.Application.Common.Interfaces
{
    /// <summary>
    /// Provides access to the current tenant context from Finbuckle.
    /// </summary>
    public interface ITenantContext
    {
        /// <summary>
        /// Gets the current tenant's internal ID (Guid).
        /// </summary>
        Guid CurrentTenantId { get; }

        /// <summary>
        /// Gets the current tenant's business identifier (e.g., "default", "infosys").
        /// </summary>
        string CurrentTenantIdentifier { get; }

        /// <summary>
        /// Checks if a tenant context is available.
        /// </summary>
        bool HasTenantContext { get; }
    }
}
