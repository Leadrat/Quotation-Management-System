using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.DocumentTemplates.Dtos;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.DocumentTemplates.Queries.Handlers
{
    public class ListTemplatesQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public ListTemplatesQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<IReadOnlyCollection<DocumentTemplateDto>> Handle(ListTemplatesQuery query, CancellationToken cancellationToken = default)
        {
            var templatesQuery = _db.QuotationTemplates
                .Include(t => t.OwnerUser)
                .Where(t => t.IsFileBased && t.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(query.TemplateType))
            {
                var normalizedType = query.TemplateType.Trim();
                templatesQuery = templatesQuery.Where(t =>
                    t.TemplateType != null &&
                    string.Equals(t.TemplateType, normalizedType, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.Equals(query.RequestedByRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                templatesQuery = templatesQuery.Where(t =>
                    t.Visibility == TemplateVisibility.Public ||
                    t.OwnerUserId == query.RequestedByUserId ||
                    t.OwnerRole == query.RequestedByRole ||
                    t.IsApproved);
            }

            var templates = await templatesQuery
                .OrderByDescending(t => t.UpdatedAt)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<DocumentTemplateDto>>(templates);
        }
    }
}

