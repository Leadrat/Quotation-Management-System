using System;
using CRM.Application.Common.Interfaces;

namespace CRM.Infrastructure.Services
{
    /// <summary>
    /// Simple implementation of ITenantContext for now.
    /// TODO: Replace with Finbuckle integration once package issues are resolved.
    /// </summary>
    public class TenantContext : ITenantContext
    {
        private readonly Guid? _currentTenantId;
        private readonly string? _currentTenantIdentifier;

        // For now, we'll use a simple implementation that reads from a static context
        // In a full implementation, this would use Finbuckle's IMultiTenantContextAccessor
        public TenantContext()
        {
            // TODO: This is a temporary implementation
            // In production, this should be injected with Finbuckle's IMultiTenantContextAccessor
            // and read from the current HTTP context's tenant information
            
            // Leadrat tenant - default tenant for all existing data
            _currentTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // Correct leadrat tenant ID from database
            _currentTenantIdentifier = "leadrat";
        }

        public Guid CurrentTenantId
        {
            get
            {
                if (!_currentTenantId.HasValue)
                    throw new InvalidOperationException("No tenant context available. Ensure X-Tenant-Id header is set.");

                return _currentTenantId.Value;
            }
        }

        public string CurrentTenantIdentifier
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_currentTenantIdentifier))
                    throw new InvalidOperationException("No tenant context available. Ensure X-Tenant-Id header is set.");

                return _currentTenantIdentifier;
            }
        }

        public bool HasTenantContext
        {
            get
            {
                return _currentTenantId.HasValue && !string.IsNullOrWhiteSpace(_currentTenantIdentifier);
            }
        }
    }
}
