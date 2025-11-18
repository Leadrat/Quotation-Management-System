using System;
using System.Text.Json;
using System.Threading.Tasks;
using CRM.Application.Clients.Commands;
using CRM.Application.Clients.Dtos;
using CRM.Application.Common.Persistence;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Clients.Commands.Handlers
{
    public class SaveSearchFilterCommandHandler
    {
        private readonly IAppDbContext _db;
        public SaveSearchFilterCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<SavedSearchDto> Handle(SaveSearchFilterCommand cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd.SearchName))
                throw new ArgumentException("SearchName is required");
            if (cmd.SearchName.Length > 255)
                throw new ArgumentException("SearchName must be <= 255 characters");
            if (cmd.FilterCriteria == null || cmd.FilterCriteria.Count == 0)
                throw new ArgumentException("FilterCriteria is required");

            var now = DateTimeOffset.UtcNow;
            var entity = new SavedSearch
            {
                SavedSearchId = Guid.NewGuid(),
                UserId = cmd.UserId,
                SearchName = cmd.SearchName.Trim(),
                FilterCriteria = JsonSerializer.Serialize(cmd.FilterCriteria),
                SortBy = string.IsNullOrWhiteSpace(cmd.SortBy) ? null : cmd.SortBy,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _db.SavedSearches.AddAsync(entity);
            await _db.SaveChangesAsync();

            return new SavedSearchDto
            {
                SavedSearchId = entity.SavedSearchId,
                SearchName = entity.SearchName,
                FilterCriteria = JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(entity.FilterCriteria) ?? new(),
                SortBy = entity.SortBy,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
