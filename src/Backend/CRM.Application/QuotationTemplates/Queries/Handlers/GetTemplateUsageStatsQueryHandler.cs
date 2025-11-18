using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.QuotationTemplates.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.QuotationTemplates.Queries.Handlers
{
    public class GetTemplateUsageStatsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<GetTemplateUsageStatsQueryHandler> _logger;

        public GetTemplateUsageStatsQueryHandler(
            IAppDbContext db,
            ILogger<GetTemplateUsageStatsQueryHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<TemplateUsageStatsDto> Handle(GetTemplateUsageStatsQuery query)
        {
            // Verify user is Admin
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == query.RequestorUserId);

            if (user == null || user.Role == null ||
                !string.Equals(user.Role.RoleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Only administrators can view usage statistics.");
            }

            var templatesQuery = _db.QuotationTemplates
                .AsNoTracking()
                .Where(t => t.DeletedAt == null);

            // Apply date filter if provided
            if (query.StartDate.HasValue)
            {
                templatesQuery = templatesQuery.Where(t =>
                    t.LastUsedAt == null || t.LastUsedAt >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                templatesQuery = templatesQuery.Where(t =>
                    t.LastUsedAt == null || t.LastUsedAt <= query.EndDate.Value);
            }

            var templates = await templatesQuery
                .Include(t => t.OwnerUser)
                .ToListAsync();

            var stats = new TemplateUsageStatsDto
            {
                TotalTemplates = templates.Count,
                TotalUsage = templates.Sum(t => t.UsageCount),
                ApprovedTemplates = templates.Count(t => t.IsApproved),
                PendingApprovalTemplates = templates.Count(t => !t.IsApproved),
                MostUsedTemplates = templates
                    .OrderByDescending(t => t.UsageCount)
                    .Take(10)
                    .Select(t => new MostUsedTemplateDto
                    {
                        TemplateId = t.TemplateId,
                        Name = t.Name,
                        UsageCount = t.UsageCount,
                        LastUsedAt = t.LastUsedAt
                    })
                    .ToList(),
                UsageByVisibility = templates
                    .GroupBy(t => t.Visibility.ToString())
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.UsageCount)),
                UsageByRole = templates
                    .GroupBy(t => t.OwnerRole)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.UsageCount))
            };

            return stats;
        }
    }
}

