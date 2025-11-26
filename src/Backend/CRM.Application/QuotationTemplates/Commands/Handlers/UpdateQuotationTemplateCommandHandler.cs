using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.QuotationTemplates.Dtos;
using CRM.Application.QuotationTemplates.Exceptions;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.QuotationTemplates.Commands.Handlers
{
    public class UpdateQuotationTemplateCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateQuotationTemplateCommandHandler> _logger;

        public UpdateQuotationTemplateCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<UpdateQuotationTemplateCommandHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<QuotationTemplateDto> Handle(UpdateQuotationTemplateCommand command)
        {
            _logger.LogInformation("Updating template {TemplateId} by user {UserId}", 
                command.TemplateId, command.UpdatedByUserId);

            // Find current template
            var currentTemplate = await _db.QuotationTemplates
                .Include(t => t.LineItems)
                .Include(t => t.OwnerUser)
                .FirstOrDefaultAsync(t => t.TemplateId == command.TemplateId);

            if (currentTemplate == null)
            {
                throw new QuotationTemplateNotFoundException(command.TemplateId);
            }

            // Check if template is deleted
            if (currentTemplate.IsDeleted())
            {
                throw new TemplateNotEditableException(command.TemplateId);
            }

            // Authorization: Owner or Admin can edit
            var isAdmin = string.Equals(command.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && currentTemplate.OwnerUserId != command.UpdatedByUserId)
            {
                throw new UnauthorizedTemplateAccessException(command.TemplateId);
            }

            // Check if template can be edited
            if (!currentTemplate.CanBeEdited())
            {
                throw new TemplateNotEditableException(command.TemplateId);
            }

            // Create new version (increment version, set PreviousVersionId)
            var newVersion = currentTemplate.Version + 1;
            var previousVersionId = currentTemplate.TemplateId;

            // Create new template entity (new version)
            var updatedTemplate = new QuotationTemplate
            {
                TemplateId = Guid.NewGuid(), // New ID for new version
                Name = command.Request.Name ?? currentTemplate.Name,
                Description = command.Request.Description ?? currentTemplate.Description,
                OwnerUserId = currentTemplate.OwnerUserId,
                OwnerRole = currentTemplate.OwnerRole,
                Visibility = !string.IsNullOrEmpty(command.Request.Visibility) 
                    ? Enum.Parse<TemplateVisibility>(command.Request.Visibility, true)
                    : currentTemplate.Visibility,
                IsApproved = currentTemplate.IsApproved, // Approval status carries over
                ApprovedByUserId = currentTemplate.ApprovedByUserId,
                ApprovedAt = currentTemplate.ApprovedAt,
                Version = newVersion,
                PreviousVersionId = previousVersionId, // Link to previous version
                UsageCount = currentTemplate.UsageCount, // Preserve usage count
                LastUsedAt = currentTemplate.LastUsedAt,
                DiscountDefault = command.Request.DiscountDefault ?? currentTemplate.DiscountDefault,
                Notes = command.Request.Notes ?? currentTemplate.Notes,
                CreatedAt = currentTemplate.CreatedAt, // Preserve original creation date
                UpdatedAt = DateTimeOffset.UtcNow
            };

            // Update line items if provided
            if (command.Request.LineItems != null && command.Request.LineItems.Any())
            {
                var lineItems = new List<QuotationTemplateLineItem>();
                for (int i = 0; i < command.Request.LineItems.Count; i++)
                {
                    var lineItemRequest = command.Request.LineItems[i];
                    QuotationTemplateLineItem lineItem;

                    if (lineItemRequest.LineItemId.HasValue)
                    {
                        // Update existing line item
                        var existingItem = currentTemplate.LineItems
                            .FirstOrDefault(li => li.LineItemId == lineItemRequest.LineItemId.Value);
                        
                        if (existingItem != null)
                        {
                            lineItem = _mapper.Map<QuotationTemplateLineItem>(lineItemRequest);
                            lineItem.LineItemId = Guid.NewGuid(); // New ID for new version to avoid PK conflicts
                            lineItem.TemplateId = updatedTemplate.TemplateId;
                            lineItem.SequenceNumber = i + 1;
                            lineItem.CalculateAmount();
                            lineItem.CreatedAt = existingItem.CreatedAt; // Preserve creation date
                        }
                        else
                        {
                            // Item not found, create new
                            lineItem = _mapper.Map<QuotationTemplateLineItem>(lineItemRequest);
                            lineItem.LineItemId = Guid.NewGuid();
                            lineItem.TemplateId = updatedTemplate.TemplateId;
                            lineItem.SequenceNumber = i + 1;
                            lineItem.CalculateAmount();
                            lineItem.CreatedAt = DateTimeOffset.UtcNow;
                        }
                    }
                    else
                    {
                        // New line item
                        lineItem = _mapper.Map<QuotationTemplateLineItem>(lineItemRequest);
                        lineItem.LineItemId = Guid.NewGuid();
                        lineItem.TemplateId = updatedTemplate.TemplateId;
                        lineItem.SequenceNumber = i + 1;
                        lineItem.CalculateAmount();
                        lineItem.CreatedAt = DateTimeOffset.UtcNow;
                    }

                    lineItems.Add(lineItem);
                }

                updatedTemplate.LineItems = lineItems;
            }
            else
            {
                // Copy existing line items to new version
                var lineItems = currentTemplate.LineItems.Select(li => new QuotationTemplateLineItem
                {
                    LineItemId = Guid.NewGuid(), // New ID for new version
                    TemplateId = updatedTemplate.TemplateId,
                    SequenceNumber = li.SequenceNumber,
                    ItemName = li.ItemName,
                    Description = li.Description,
                    Quantity = li.Quantity,
                    UnitRate = li.UnitRate,
                    Amount = li.Amount,
                    CreatedAt = li.CreatedAt
                }).ToList();

                updatedTemplate.LineItems = lineItems;
            }

            // Save new version
            _db.QuotationTemplates.Add(updatedTemplate);
            await _db.SaveChangesAsync();

            // Load with navigation properties
            var savedTemplate = await _db.QuotationTemplates
                .Include(t => t.OwnerUser)
                .Include(t => t.ApprovedByUser)
                .Include(t => t.LineItems)
                .FirstOrDefaultAsync(t => t.TemplateId == updatedTemplate.TemplateId);

            // Map to DTO
            var result = _mapper.Map<QuotationTemplateDto>(savedTemplate);

            _logger.LogInformation("Template {TemplateId} updated to version {Version}", 
                result.TemplateId, result.Version);

            return result;
        }
    }
}

