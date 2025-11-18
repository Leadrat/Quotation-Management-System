using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Reports.Commands;
using CRM.Application.Reports.Commands.Handlers;
using CRM.Application.Reports.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/dashboard/bookmarks")]
    [Authorize]
    public class DashboardBookmarksController : ControllerBase
    {
        private readonly SaveDashboardBookmarkCommandHandler _saveHandler;
        private readonly DeleteDashboardBookmarkCommandHandler _deleteHandler;
        private readonly IAppDbContext _db;
        private readonly IValidator<SaveDashboardBookmarkCommand> _saveValidator;

        public DashboardBookmarksController(
            SaveDashboardBookmarkCommandHandler saveHandler,
            DeleteDashboardBookmarkCommandHandler deleteHandler,
            IAppDbContext db,
            IValidator<SaveDashboardBookmarkCommand> saveValidator)
        {
            _saveHandler = saveHandler;
            _deleteHandler = deleteHandler;
            _db = db;
            _saveValidator = saveValidator;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SaveBookmark([FromBody] SaveDashboardBookmarkRequest request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var command = new SaveDashboardBookmarkCommand
            {
                DashboardConfig = request.DashboardConfig,
                DashboardName = request.DashboardName,
                IsDefault = request.IsDefault,
                UserId = userId,
                BookmarkId = request.BookmarkId
            };

            var validation = await _saveValidator.ValidateAsync(command);
            if (!validation.IsValid)
            {
                return BadRequest(new { success = false, errors = validation.ToDictionary() });
            }

            var result = await _saveHandler.Handle(command);
            return Ok(new { success = true, data = result });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetBookmarks()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var bookmarks = await _db.DashboardBookmarks
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.IsDefault)
                .ThenByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    b.BookmarkId,
                    b.DashboardName,
                    b.DashboardConfig,
                    b.IsDefault,
                    b.CreatedAt,
                    b.UpdatedAt
                })
                .ToListAsync();

            return Ok(new { success = true, data = bookmarks });
        }

        [HttpDelete("{bookmarkId}")]
        [Authorize]
        public async Task<IActionResult> DeleteBookmark([FromRoute] Guid bookmarkId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var command = new DeleteDashboardBookmarkCommand
            {
                BookmarkId = bookmarkId,
                UserId = userId
            };

            await _deleteHandler.Handle(command);
            return Ok(new { success = true, message = "Bookmark deleted" });
        }
    }

    public class SaveDashboardBookmarkRequest
    {
        public DashboardConfig DashboardConfig { get; set; } = null!;
        public string DashboardName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public Guid? BookmarkId { get; set; }
    }
}

