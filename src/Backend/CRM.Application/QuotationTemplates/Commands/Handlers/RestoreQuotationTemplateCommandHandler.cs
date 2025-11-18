using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.QuotationTemplates.Dtos;
using CRM.Application.QuotationTemplates.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.QuotationTemplates.Commands.Handlers
{
    public class RestoreQuotationTemplateCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<RestoreQuotationTemplateCommandHandler> _logger;

        public RestoreQuotationTemplateCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<RestoreQuotationTemplateCommandHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<QuotationTemplateDto> Handle(RestoreQuotationTemplateCommand command)
        {
            _logger.LogInformation("Restoring template {TemplateId} by user {UserId}", 
                command.TemplateId, command.RestoredByUserId);

            var template = await _db.QuotationTemplates
                .Include(t => t.OwnerUser)
                .Include(t => t.LineItems)
                .FirstOrDefaultAsync(t => t.TemplateId == command.TemplateId);

            if (template == null)
            {
                throw new QuotationTemplateNotFoundException(command.TemplateId);
            }

            // Check if template is deleted
            if (!template.IsDeleted())
            {
                _logger.LogWarning("Template {TemplateId} is not deleted", command.TemplateId);
                // Return the template as-is
                return _mapper.Map<QuotationTemplateDto>(template);
            }

            // Authorization: Owner or Admin can restore
            var isAdmin = string.Equals(command.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && template.OwnerUserId != command.RestoredByUserId)
            {
                throw new UnauthorizedTemplateAccessException(command.TemplateId);
            }

            // Restore (clear DeletedAt)
            template.DeletedAt = null;
            await _db.SaveChangesAsync();

            // Reload with navigation properties
            var restoredTemplate = await _db.QuotationTemplates
                .Include(t => t.OwnerUser)
                .Include(t => t.ApprovedByUser)
                .Include(t => t.LineItems)
                .FirstOrDefaultAsync(t => t.TemplateId == template.TemplateId);

            var result = _mapper.Map<QuotationTemplateDto>(restoredTemplate);

            _logger.LogInformation("Template {TemplateId} restored successfully", command.TemplateId);

            return result;
        }
    }
}

