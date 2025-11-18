using System;
using System.Text.Json;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Reports.Commands.Handlers
{
    public class SaveDashboardBookmarkCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<SaveDashboardBookmarkCommandHandler> _logger;

        public SaveDashboardBookmarkCommandHandler(IAppDbContext db, ILogger<SaveDashboardBookmarkCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<DashboardBookmark> Handle(SaveDashboardBookmarkCommand command)
        {
            DashboardBookmark bookmark;

            if (command.BookmarkId.HasValue)
            {
                // Update existing bookmark
                bookmark = await _db.DashboardBookmarks
                    .FirstOrDefaultAsync(b => b.BookmarkId == command.BookmarkId.Value && 
                                              b.UserId == command.UserId);

                if (bookmark == null)
                {
                    throw new InvalidOperationException($"Dashboard bookmark not found: {command.BookmarkId.Value}");
                }

                bookmark.DashboardName = command.DashboardName;
                bookmark.DashboardConfig = JsonDocument.Parse(JsonSerializer.Serialize(command.DashboardConfig));
                bookmark.IsDefault = command.IsDefault;
                bookmark.UpdatedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                // Create new bookmark
                bookmark = new DashboardBookmark
                {
                    BookmarkId = Guid.NewGuid(),
                    UserId = command.UserId,
                    DashboardName = command.DashboardName,
                    DashboardConfig = JsonDocument.Parse(JsonSerializer.Serialize(command.DashboardConfig)),
                    IsDefault = command.IsDefault,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                _db.DashboardBookmarks.Add(bookmark);
            }

            // If this is set as default, unset other defaults for this user
            if (command.IsDefault)
            {
                var otherDefaults = await _db.DashboardBookmarks
                    .Where(b => b.UserId == command.UserId && 
                               b.BookmarkId != bookmark.BookmarkId && 
                               b.IsDefault)
                    .ToListAsync();

                foreach (var other in otherDefaults)
                {
                    other.IsDefault = false;
                    other.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Dashboard bookmark saved: {BookmarkId}, User: {UserId}, Default: {IsDefault}",
                bookmark.BookmarkId, command.UserId, command.IsDefault);

            return bookmark;
        }
    }
}

