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
    public class ApproveQuotationTemplateCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<ApproveQuotationTemplateCommandHandler> _logger;

        public ApproveQuotationTemplateCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<ApproveQuotationTemplateCommandHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<QuotationTemplateDto> Handle(ApproveQuotationTemplateCommand command)
        {
            _logger.LogInformation("Approving template {TemplateId} by user {UserId}", 
                command.TemplateId, command.ApprovedByUserId);

            // Verify user is Admin
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == command.ApprovedByUserId);

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {command.ApprovedByUserId} not found.");
            }

            var isAdmin = user.Role != null && 
                string.Equals(user.Role.RoleName, "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin)
            {
                throw new UnauthorizedAccessException("Only administrators can approve templates.");
            }

            // Find template (get latest version)
            var template = await _db.QuotationTemplates
                .Include(t => t.OwnerUser)
                .Include(t => t.LineItems)
                .FirstOrDefaultAsync(t => t.TemplateId == command.TemplateId);

            if (template == null)
            {
                throw new QuotationTemplateNotFoundException(command.TemplateId);
            }

            // Check if template is deleted
            if (template.IsDeleted())
            {
                throw new TemplateNotEditableException(command.TemplateId);
            }

            // Approve template
            template.MarkAsApproved(command.ApprovedByUserId);
            await _db.SaveChangesAsync();

            // Reload with navigation properties
            var approvedTemplate = await _db.QuotationTemplates
                .Include(t => t.OwnerUser)
                .Include(t => t.ApprovedByUser)
                .Include(t => t.LineItems)
                .FirstOrDefaultAsync(t => t.TemplateId == template.TemplateId);

            var result = _mapper.Map<QuotationTemplateDto>(approvedTemplate);

            _logger.LogInformation("Template {TemplateId} approved successfully", command.TemplateId);

            return result;
        }
    }
}

