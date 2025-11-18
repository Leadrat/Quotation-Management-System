using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using CRM.Application.Clients.Dtos;
using CRM.Application.Clients.Queries;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Clients.Queries.Handlers
{
    public class GetSavedSearchesQueryHandler
    {
        private readonly IAppDbContext _db;
        public GetSavedSearchesQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<List<SavedSearchDto>> Handle(GetSavedSearchesQuery q)
        {
            var userId = q.IsAdmin && q.UserId.HasValue ? q.UserId.Value : q.RequestorUserId;

            var list = await _db.SavedSearches.AsNoTracking()
                .Where(s => s.IsActive && s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new { s.SavedSearchId, s.SearchName, s.FilterCriteria, s.SortBy, s.CreatedAt })
                .ToListAsync();

            return list.Select(s => new SavedSearchDto
            {
                SavedSearchId = s.SavedSearchId,
                SearchName = s.SearchName,
                FilterCriteria = JsonSerializer.Deserialize<Dictionary<string, object>>(s.FilterCriteria) ?? new(),
                SortBy = s.SortBy,
                CreatedAt = s.CreatedAt
            }).ToList();
        }
    }
}
