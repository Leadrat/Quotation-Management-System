using System;
using System.Text.Json;

namespace CRM.Domain.Entities
{
    /// <summary>
    /// Saves user dashboard configurations for quick access
    /// </summary>
    public class DashboardBookmark
    {
        public Guid BookmarkId { get; set; }
        public Guid UserId { get; set; }
        public string DashboardName { get; set; } = string.Empty;
        public JsonDocument DashboardConfig { get; set; } = null!;
        public bool IsDefault { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;
    }
}

