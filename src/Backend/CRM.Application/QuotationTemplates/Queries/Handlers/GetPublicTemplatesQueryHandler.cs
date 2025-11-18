using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.QuotationTemplates.Dtos;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.QuotationTemplates.Queries.Handlers
{
    public class GetPublicTemplatesQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPublicTemplatesQueryHandler> _logger;

        public GetPublicTemplatesQueryHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<GetPublicTemplatesQueryHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<QuotationTemplateDto>> Handle(GetPublicTemplatesQuery query)
        {
            var isAdmin = string.Equals(query.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);

            // Query templates visible to user
            IQueryable<Domain.Entities.QuotationTemplate> templatesQuery = _db.QuotationTemplates
                .AsNoTracking()
                .Where(t => t.DeletedAt == null);

            if (!isAdmin)
            {
                // Non-admin: Public approved, Team with matching role, or own Private
                templatesQuery = templatesQuery.Where(t =>
                    (t.Visibility == TemplateVisibility.Public && t.IsApproved) ||
                    (t.Visibility == TemplateVisibility.Team && t.OwnerRole == query.RequestorRole) ||
                    (t.Visibility == TemplateVisibility.Private && t.OwnerUserId == query.RequestorUserId));
            }

            // Include navigation properties after filtering
            templatesQuery = templatesQuery
                .Include(t => t.OwnerUser)
                .Include(t => t.LineItems);

            // Order by usage count DESC, then name ASC
            var templates = await templatesQuery
                .OrderByDescending(t => t.UsageCount)
                .ThenBy(t => t.Name)
                .ToListAsync();

            return templates.Select(t => _mapper.Map<QuotationTemplateDto>(t)).ToList();
        }
    }
}

