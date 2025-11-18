using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("SavedSearches")]
    public class SavedSearch
    {
        public Guid SavedSearchId { get; set; }
        public Guid UserId { get; set; }
        public string SearchName { get; set; } = string.Empty;
        [Column(TypeName = "jsonb")]
        public string FilterCriteria { get; set; } = string.Empty; // JSON string stored as jsonb
        public string? SortBy { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
