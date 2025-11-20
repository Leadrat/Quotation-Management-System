using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using CRM.Application.Clients.Queries;
using CRM.Application.Clients.Queries.Handlers;
using CRM.Application.Common.Persistence;
using Microsoft.Extensions.Caching.Memory;
using CRM.Application.Clients.Commands;
using CRM.Application.Clients.Commands.Handlers;
using CRM.Application.Clients.Dtos;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/clients")]
    public class ClientsSearchController : ControllerBase
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public ClientsSearchController(IAppDbContext db, IMapper mapper, IMemoryCache cache)
        {
            _db = db;
            _mapper = mapper;
            _cache = cache;
        }

        public class SearchRequest
        {
            public string? SearchTerm { get; set; }
            public string? City { get; set; }
            public string? State { get; set; }
            public string? StateCode { get; set; }
            public string? Gstin { get; set; }
            public Guid? UserId { get; set; }
            public DateTimeOffset? CreatedDateFrom { get; set; }
            public DateTimeOffset? CreatedDateTo { get; set; }
            public DateTimeOffset? UpdatedDateFrom { get; set; }
            public DateTimeOffset? UpdatedDateTo { get; set; }
            public string SortBy { get; set; } = "CreatedAtDesc";
            public int PageNumber { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        }

        [HttpGet("search")]
        [Authorize(Roles = "SalesRep,Admin")]
        public async Task<IActionResult> Search([FromQuery] SearchRequest req)
        {
            try
            {
                // Normalize paging
                if (req.PageNumber < 1) req.PageNumber = 1;
                if (req.PageSize < 1) req.PageSize = 10;
                if (req.PageSize > 100) req.PageSize = 100;

                // Build query object from request + user context
                var user = HttpContext.User;
                var isAdmin = user.IsInRole("Admin");
                var role = isAdmin ? "Admin" : (user.IsInRole("SalesRep") ? "SalesRep" : string.Empty);
                
                // Try multiple claim types to find user ID
                var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value
                    ?? user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                    ?? user.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId) || currentUserId == Guid.Empty)
                {
                    var availableClaims = string.Join(", ", user.Claims.Select(c => $"{c.Type}={c.Value}"));
                    return Unauthorized(new { success = false, error = "Invalid user token - user ID not found", details = System.Diagnostics.Debugger.IsAttached ? $"Available claims: {availableClaims}" : null });
                }

                var query = new SearchClientsQuery
                {
                    SearchTerm = req.SearchTerm,
                    City = req.City,
                    State = req.State,
                    StateCode = req.StateCode,
                    Gstin = req.Gstin,
                    CreatedByUserId = isAdmin ? req.UserId : null,
                    CreatedDateFrom = req.CreatedDateFrom,
                    CreatedDateTo = req.CreatedDateTo,
                    UpdatedDateFrom = req.UpdatedDateFrom,
                    UpdatedDateTo = req.UpdatedDateTo,
                    SortBy = req.SortBy,
                    PageNumber = req.PageNumber,
                    PageSize = req.PageSize,
                    IncludeDeleted = false,
                    RequestorUserId = currentUserId,
                    RequestorRole = role
                };

                var sw = Stopwatch.StartNew();
                var handler = new SearchClientsQueryHandler(_db, _mapper);
                var result = await handler.Handle(query);
                sw.Stop();

                return Ok(new
                {
                    success = true,
                    data = result.Data,
                    pageNumber = result.PageNumber,
                    pageSize = result.PageSize,
                    totalCount = result.TotalCount,
                    hasMore = (result.PageNumber * result.PageSize) < result.TotalCount,
                    searchExecutedIn = $"{sw.ElapsedMilliseconds}ms"
                });
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging
                var innerException = ex.InnerException != null ? $" Inner: {ex.InnerException.Message}" : "";
                return StatusCode(500, new { 
                    success = false, 
                    error = "An error occurred while searching clients", 
                    message = $"{ex.Message}{innerException}",
                    stackTrace = System.Diagnostics.Debugger.IsAttached ? ex.StackTrace : null 
                });
            }
        }

        [HttpGet("search/suggestions")]
        [Authorize(Roles = "SalesRep,Admin")]
        public async Task<IActionResult> Suggestions([FromQuery] string term, [FromQuery] string type = "CompanyName", [FromQuery] int maxSuggestions = 10)
        {
            var user = HttpContext.User;
            var isAdmin = user.IsInRole("Admin");
            var role = isAdmin ? "Admin" : (user.IsInRole("SalesRep") ? "SalesRep" : string.Empty);
            Guid.TryParse(user.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "userId")?.Value, out var currentUserId);

            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return BadRequest(new { success = false, error = "Search term must be at least 2 characters" });
            }
            if (maxSuggestions < 1) maxSuggestions = 10; if (maxSuggestions > 50) maxSuggestions = 50;

            var q = new GetClientSearchSuggestionsQuery
            {
                SearchTerm = term,
                MaxSuggestions = maxSuggestions,
                Type = Enum.TryParse<SuggestionType>(type, true, out var t) ? t : SuggestionType.CompanyName,
                RequestorUserId = currentUserId,
                RequestorRole = role
            };
            var handler = new GetClientSearchSuggestionsQueryHandler(_db);
            var data = await handler.Handle(q);
            return Ok(new { success = true, data });
        }

        [HttpGet("search/filter-options")]
        [Authorize(Roles = "SalesRep,Admin")]
        public async Task<IActionResult> FilterOptions()
        {
            var cacheKey = "client-filter-options:v1";
            if (!_cache.TryGetValue(cacheKey, out FilterOptionsDto? options))
            {
                var handler = new GetFilterOptionsQueryHandler(_db);
                options = await handler.Handle(new GetFilterOptionsQuery());
                var entry = _cache.CreateEntry(cacheKey);
                entry.Value = options;
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
                entry.Dispose();
            }
            return Ok(new { success = true, data = options });
        }

        public class SaveSearchBody
        {
            public string SearchName { get; set; } = string.Empty;
            public System.Collections.Generic.Dictionary<string, object> FilterCriteria { get; set; } = new();
            public string? SortBy { get; set; }
        }

        [HttpPost("search/save")]
        [Authorize(Roles = "SalesRep,Admin")]
        public async Task<IActionResult> SaveSearch([FromBody] SaveSearchBody body)
        {
            var user = HttpContext.User;
            Guid.TryParse(user.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "userId")?.Value, out var currentUserId);

            var handler = new SaveSearchFilterCommandHandler(_db);
            var dto = await handler.Handle(new SaveSearchFilterCommand
            {
                SearchName = body.SearchName,
                FilterCriteria = body.FilterCriteria,
                SortBy = body.SortBy,
                UserId = currentUserId
            });
            return StatusCode(201, new { success = true, message = "Search saved successfully", data = dto });
        }

        [HttpGet("search/saved")]
        [Authorize(Roles = "SalesRep,Admin")]
        public async Task<IActionResult> GetSaved([FromQuery] Guid? userId = null)
        {
            var user = HttpContext.User;
            var isAdmin = user.IsInRole("Admin");
            Guid.TryParse(user.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "userId")?.Value, out var currentUserId);

            var handler = new GetSavedSearchesQueryHandler(_db);
            var list = await handler.Handle(new GetSavedSearchesQuery
            {
                RequestorUserId = currentUserId,
                IsAdmin = isAdmin,
                UserId = userId
            });
            return Ok(new { success = true, data = list });
        }

        [HttpDelete("search/saved/{savedSearchId}")]
        [Authorize(Roles = "SalesRep,Admin")]
        public async Task<IActionResult> DeleteSaved([FromRoute] Guid savedSearchId)
        {
            var user = HttpContext.User;
            var isAdmin = user.IsInRole("Admin");
            Guid.TryParse(user.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "userId")?.Value, out var currentUserId);

            var handler = new DeleteSavedSearchCommandHandler(_db);
            await handler.Handle(new DeleteSavedSearchCommand
            {
                SavedSearchId = savedSearchId,
                UserId = currentUserId,
                IsAdmin = isAdmin
            });
            return Ok(new { success = true, message = "Saved search deleted successfully" });
        }

        [HttpGet("export")]
        [Authorize(Roles = "SalesRep,Admin")]
        public async Task<IActionResult> Export(
            [FromQuery] string? searchTerm,
            [FromQuery] string? city,
            [FromQuery] string? state,
            [FromQuery] string? stateCode,
            [FromQuery] string? gstin,
            [FromQuery] Guid? userId,
            [FromQuery] string format = "csv")
        {
            var user = HttpContext.User;
            var isAdmin = user.IsInRole("Admin");
            var role = isAdmin ? "Admin" : (user.IsInRole("SalesRep") ? "SalesRep" : string.Empty);
            Guid.TryParse(user.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "userId")?.Value, out var currentUserId);

            var handler = new ExportClientsQueryHandler(_db);
            var data = await handler.Handle(new ExportClientsQuery
            {
                SearchTerm = searchTerm,
                City = city,
                State = state,
                StateCode = stateCode,
                Gstin = gstin,
                CreatedByUserId = isAdmin ? userId : null,
                RequestorUserId = currentUserId,
                RequestorRole = role,
                Format = string.IsNullOrWhiteSpace(format) ? "csv" : format.ToLower(),
                MaxRows = 10000
            });

            var now = DateTime.UtcNow;
            var timestamp = now.ToString("yyyyMMdd_HHmmss");

            if (format.Equals("excel", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { success = false, error = "Excel export not available in Phase 1. Use format=csv." });
            }

            Response.ContentType = "text/csv";
            Response.Headers["Content-Disposition"] = $"attachment; filename=clients_{timestamp}.csv";

            await Response.WriteAsync("ClientId,CompanyName,ContactName,Email,Mobile,GSTIN,State,City,CreatedAt,UpdatedAt\n");
            foreach (var c in data)
            {
                string Csv(string? s) => s == null ? "" : (s.Contains(',') || s.Contains('"') || s.Contains('\n') ? '"' + s.Replace("\"", "\"\"") + '"' : s);
                await Response.WriteAsync(string.Join(',', new[]
                {
                    c.ClientId.ToString(),
                    Csv(c.CompanyName),
                    Csv(c.ContactName ?? ""),
                    Csv(c.Email),
                    Csv(c.Mobile),
                    Csv(c.Gstin ?? ""),
                    Csv(c.State ?? ""),
                    Csv(c.City ?? ""),
                    c.CreatedAt.ToString("o"),
                    c.UpdatedAt.ToString("o")
                }) + "\n");
            }
            return new EmptyResult();
        }
    }
}
