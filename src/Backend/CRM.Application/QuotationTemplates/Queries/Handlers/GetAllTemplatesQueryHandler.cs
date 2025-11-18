using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using CRM.Application.QuotationTemplates.Dtos;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.QuotationTemplates.Queries.Handlers
{
    public class GetAllTemplatesQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllTemplatesQueryHandler> _logger;

        public GetAllTemplatesQueryHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<GetAllTemplatesQueryHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<QuotationTemplateDto>> Handle(GetAllTemplatesQuery query)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize > 100 ? 100 : (query.PageSize < 1 ? 10 : query.PageSize);

            var isAdmin = string.Equals(query.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);

            // Base query - exclude deleted templates
            IQueryable<Domain.Entities.QuotationTemplate> baseQuery = _db.QuotationTemplates
                .AsNoTracking()
                .Where(t => t.DeletedAt == null);

            // Apply visibility/authorization filters
            if (!isAdmin)
            {
                // Non-admin users can see:
                // 1. Their own templates (any visibility)
                // 2. Public approved templates
                // 3. Team templates where owner role matches
                baseQuery = baseQuery.Where(t =>
                    t.OwnerUserId == query.RequestorUserId ||
                    (t.Visibility == TemplateVisibility.Public && t.IsApproved) ||
                    (t.Visibility == TemplateVisibility.Team && t.OwnerRole == query.RequestorRole));
            }

            // Apply filters
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var searchTerm = query.Search.ToLower();
                baseQuery = baseQuery.Where(t =>
                    t.Name.ToLower().Contains(searchTerm) ||
                    (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(query.Visibility))
            {
                if (Enum.TryParse<TemplateVisibility>(query.Visibility, true, out var visibility))
                {
                    baseQuery = baseQuery.Where(t => t.Visibility == visibility);
                }
            }

            if (query.IsApproved.HasValue)
            {
                baseQuery = baseQuery.Where(t => t.IsApproved == query.IsApproved.Value);
            }

            if (query.OwnerUserId.HasValue)
            {
                baseQuery = baseQuery.Where(t => t.OwnerUserId == query.OwnerUserId.Value);
            }

            // Include navigation properties after filtering
            baseQuery = baseQuery
                .Include(t => t.OwnerUser)
                .Include(t => t.ApprovedByUser)
                .Include(t => t.LineItems);

            // Get total count
            var totalCount = await baseQuery.CountAsync();

            // Apply pagination and ordering
            var templates = await baseQuery
                .OrderByDescending(t => t.UpdatedAt)
                .ThenBy(t => t.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = templates.Select(t => _mapper.Map<QuotationTemplateDto>(t)).ToArray();

            return new PagedResult<QuotationTemplateDto>
            {
                Data = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}

