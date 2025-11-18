using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CRM.Application.Clients.Queries;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Clients.Queries.Handlers
{
    public class GetClientSearchSuggestionsQueryHandler
    {
        private readonly IAppDbContext _db;
        public GetClientSearchSuggestionsQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<List<string>> Handle(GetClientSearchSuggestionsQuery q)
        {
            var isAdmin = string.Equals(q.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            var term = q.SearchTerm.Trim();
            var lower = term.ToLower();

            var clients = _db.Clients.AsNoTracking();
            if (!isAdmin)
            {
                clients = clients.Where(c => c.CreatedByUserId == q.RequestorUserId);
            }

            IQueryable<string?> proj = q.Type switch
            {
                SuggestionType.Email => clients.Select(c => c.Email),
                SuggestionType.City => clients.Select(c => c.City),
                SuggestionType.ContactName => clients.Select(c => c.ContactName),
                _ => clients.Select(c => c.CompanyName)
            };

            // Filter by contains to reduce set
            var filtered = proj.Where(v => v != null && v.ToLower().Contains(lower));

            // Order prefix matches first, then contains
            var ordered = filtered
                .Select(v => v!)
                .Distinct()
                .OrderByDescending(v => v.ToLower().StartsWith(lower))
                .ThenBy(v => v);

            return await ordered.Take(q.MaxSuggestions).ToListAsync();
        }
    }
}
