using System;
using System.Collections.Generic;

namespace CRM.Application.Clients.Commands
{
    public class SaveSearchFilterCommand
    {
        public string SearchName { get; set; } = string.Empty;
        public Dictionary<string, object> FilterCriteria { get; set; } = new();
        public string? SortBy { get; set; }
        public Guid UserId { get; set; }
    }
}
