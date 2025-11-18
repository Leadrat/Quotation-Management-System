using System;

namespace CRM.Application.Clients.Queries
{
    public enum SuggestionType
    {
        CompanyName,
        Email,
        City,
        ContactName
    }

    public class GetClientSearchSuggestionsQuery
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int MaxSuggestions { get; set; } = 10;
        public SuggestionType Type { get; set; } = SuggestionType.CompanyName;

        // Requestor context
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}
