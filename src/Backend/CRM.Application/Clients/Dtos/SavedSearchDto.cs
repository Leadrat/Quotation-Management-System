using System;
using System.Collections.Generic;

namespace CRM.Application.Clients.Dtos
{
    public class SavedSearchDto
    {
        public Guid SavedSearchId { get; set; }
        public string SearchName { get; set; } = string.Empty;
        public Dictionary<string, object> FilterCriteria { get; set; } = new();
        public string? SortBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
