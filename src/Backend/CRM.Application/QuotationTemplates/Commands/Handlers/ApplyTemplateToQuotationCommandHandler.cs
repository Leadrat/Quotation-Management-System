using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.QuotationTemplates.Exceptions;
using CRM.Application.Quotations.Dtos;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.QuotationTemplates.Commands.Handlers
{
    public class ApplyTemplateToQuotationCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<ApplyTemplateToQuotationCommandHandler> _logger;

        public ApplyTemplateToQuotationCommandHandler(
            IAppDbContext db,
            ILogger<ApplyTemplateToQuotationCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<CreateQuotationRequest> Handle(ApplyTemplateToQuotationCommand command)
        {
            _logger.LogInformation("Applying template {TemplateId} to client {ClientId} by user {UserId}", 
                command.TemplateId, command.ClientId, command.AppliedByUserId);

            // Find template (get latest version)
            var template = await _db.QuotationTemplates
                .Include(t => t.LineItems.OrderBy(li => li.SequenceNumber))
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

            // Check visibility rules
            var canAccess = CanAccessTemplate(template, command.AppliedByUserId, command.RequestorRole);
            if (!canAccess)
            {
                throw new UnauthorizedTemplateAccessException(command.TemplateId);
            }

            // Verify client exists
            var client = await _db.Clients
                .FirstOrDefaultAsync(c => c.ClientId == command.ClientId);

            if (client == null)
            {
                throw new InvalidOperationException($"Client with ID {command.ClientId} not found.");
            }

            // Increment usage count
            template.UsageCount++;
            template.LastUsedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

            // Convert template to CreateQuotationRequest
            var request = new CreateQuotationRequest
            {
                ClientId = command.ClientId,
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30), // Default 30 days
                DiscountPercentage = template.DiscountDefault ?? 0,
                Notes = template.Notes,
                LineItems = template.LineItems.Select(li => new CreateLineItemRequest
                {
                    ItemName = li.ItemName,
                    Description = li.Description,
                    Quantity = li.Quantity,
                    UnitRate = li.UnitRate
                }).ToList()
            };

            _logger.LogInformation("Template {TemplateId} applied successfully. Usage count: {UsageCount}", 
                command.TemplateId, template.UsageCount);

            return request;
        }

        private bool CanAccessTemplate(QuotationTemplate template, Guid userId, string userRole)
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
                    // Public templates must be approved
                    return template.IsApproved;

                case TemplateVisibility.Team:
                    // Team templates: same role as owner
                    return string.Equals(template.OwnerRole, userRole, StringComparison.OrdinalIgnoreCase);

                case TemplateVisibility.Private:
                    // Private templates: only owner (already checked above)
                    return false;

                default:
                    return false;
            }
        }
    }
}

