using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.QuotationTemplates.Dtos;
using CRM.Application.QuotationTemplates.Exceptions;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.QuotationTemplates.Queries.Handlers
{
    public class GetTemplateByIdQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<GetTemplateByIdQueryHandler> _logger;

        public GetTemplateByIdQueryHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<GetTemplateByIdQueryHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<QuotationTemplateDto> Handle(GetTemplateByIdQuery query)
        {
            var template = await _db.QuotationTemplates
                .Include(t => t.OwnerUser)
                .Include(t => t.ApprovedByUser)
                .Include(t => t.LineItems.OrderBy(li => li.SequenceNumber))
                .FirstOrDefaultAsync(t => t.TemplateId == query.TemplateId);

            if (template == null)
            {
                throw new QuotationTemplateNotFoundException(query.TemplateId);
            }

            // Check visibility and authorization
            if (!CanAccessTemplate(template, query.RequestorUserId, query.RequestorRole))
            {
                throw new UnauthorizedTemplateAccessException(query.TemplateId);
            }

            // Exclude deleted templates (unless admin viewing for restore)
            var isAdmin = string.Equals(query.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && template.IsDeleted())
            {
                throw new QuotationTemplateNotFoundException(query.TemplateId);
            }

            return _mapper.Map<QuotationTemplateDto>(template);
        }

        private bool CanAccessTemplate(Domain.Entities.QuotationTemplate template, Guid userId, string userRole)
        {
            // Admin can access all templates
            if (string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Owner can always access their templates
            if (template.OwnerUserId == userId)
            {
                return true;
            }

            // Check visibility rules
            switch (template.Visibility)
            {
                case TemplateVisibility.Public:
                    return template.IsApproved;

                case TemplateVisibility.Team:
                    return string.Equals(template.OwnerRole, userRole, StringComparison.OrdinalIgnoreCase);

                case TemplateVisibility.Private:
                    return false;

                default:
                    return false;
            }
        }
    }
}

