using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Reports.Commands.Handlers
{
    public class DeleteDashboardBookmarkCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<DeleteDashboardBookmarkCommandHandler> _logger;

        public DeleteDashboardBookmarkCommandHandler(IAppDbContext db, ILogger<DeleteDashboardBookmarkCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Handle(DeleteDashboardBookmarkCommand command)
        {
            var bookmark = await _db.DashboardBookmarks
                .FirstOrDefaultAsync(b => b.BookmarkId == command.BookmarkId && 
                                         b.UserId == command.UserId);

            if (bookmark == null)
            {
                throw new InvalidOperationException($"Dashboard bookmark not found: {command.BookmarkId}");
            }

            _db.DashboardBookmarks.Remove(bookmark);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Dashboard bookmark deleted: {BookmarkId}, User: {UserId}",
                command.BookmarkId, command.UserId);
        }
    }
}

