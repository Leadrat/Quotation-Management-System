using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.QuotationTemplates.Dtos;
using CRM.Application.QuotationTemplates.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.QuotationTemplates.Queries.Handlers
{
    public class GetTemplateVersionsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<GetTemplateVersionsQueryHandler> _logger;

        public GetTemplateVersionsQueryHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<GetTemplateVersionsQueryHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<QuotationTemplateVersionDto>> Handle(GetTemplateVersionsQuery query)
        {
            // Find current template
            var currentTemplate = await _db.QuotationTemplates
                .Include(t => t.OwnerUser)
                .FirstOrDefaultAsync(t => t.TemplateId == query.TemplateId);

            if (currentTemplate == null)
            {
                throw new QuotationTemplateNotFoundException(query.TemplateId);
            }

            // Authorization: Owner can view version history
            if (currentTemplate.OwnerUserId != query.RequestorUserId)
            {
                throw new UnauthorizedTemplateAccessException(query.TemplateId);
            }

            // Traverse version chain via PreviousVersionId
            var versions = new List<Domain.Entities.QuotationTemplate>();
            var visited = new HashSet<Guid>();

            var template = currentTemplate;
            while (template != null && !visited.Contains(template.TemplateId))
            {
                visited.Add(template.TemplateId);
                versions.Add(template);

                if (template.PreviousVersionId.HasValue)
                {
                    template = await _db.QuotationTemplates
                        .Include(t => t.OwnerUser)
                        .FirstOrDefaultAsync(t => t.TemplateId == template.PreviousVersionId.Value);
                }
                else
                {
                    template = null;
                }
            }

            // Order by version DESC (newest first)
            versions = versions.OrderByDescending(v => v.Version).ToList();

            // Map to DTOs
            var result = versions.Select((v, index) =>
            {
                var dto = _mapper.Map<QuotationTemplateVersionDto>(v);
                dto.IsCurrentVersion = index == 0; // First item is current version
                return dto;
            }).ToList();

            return result;
        }
    }
}

