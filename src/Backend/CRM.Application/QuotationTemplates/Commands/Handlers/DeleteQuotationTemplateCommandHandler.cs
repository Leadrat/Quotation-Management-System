using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.QuotationTemplates.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.QuotationTemplates.Commands.Handlers
{
    public class DeleteQuotationTemplateCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<DeleteQuotationTemplateCommandHandler> _logger;

        public DeleteQuotationTemplateCommandHandler(
            IAppDbContext db,
            ILogger<DeleteQuotationTemplateCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Handle(DeleteQuotationTemplateCommand command)
        {
            _logger.LogInformation("Deleting template {TemplateId} by user {UserId}", 
                command.TemplateId, command.DeletedByUserId);

            var template = await _db.QuotationTemplates
                .FirstOrDefaultAsync(t => t.TemplateId == command.TemplateId);

            if (template == null)
            {
                throw new QuotationTemplateNotFoundException(command.TemplateId);
            }

            // Check if already deleted
            if (template.IsDeleted())
            {
                _logger.LogWarning("Template {TemplateId} is already deleted", command.TemplateId);
                return;
            }

            // Authorization: Owner or Admin can delete
            var isAdmin = string.Equals(command.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && template.OwnerUserId != command.DeletedByUserId)
            {
                throw new UnauthorizedTemplateAccessException(command.TemplateId);
            }

            // Soft delete
            template.DeletedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Template {TemplateId} deleted successfully", command.TemplateId);
        }
    }
}

