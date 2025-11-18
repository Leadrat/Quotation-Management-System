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
    public class CreateQuotationTemplateCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateQuotationTemplateCommandHandler> _logger;

        public CreateQuotationTemplateCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<CreateQuotationTemplateCommandHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<QuotationTemplateDto> Handle(CreateQuotationTemplateCommand command)
        {
            _logger.LogInformation("Creating quotation template '{Name}' by user {UserId}", 
                command.Request.Name, command.CreatedByUserId);

            // Validate visibility
            if (!Enum.TryParse<TemplateVisibility>(command.Request.Visibility, true, out var visibility))
            {
                throw new InvalidTemplateVisibilityException(command.Request.Visibility);
            }

            // Check name uniqueness per owner
            var existingTemplate = await _db.QuotationTemplates
                .FirstOrDefaultAsync(t => 
                    t.Name == command.Request.Name && 
                    t.OwnerUserId == command.CreatedByUserId &&
                    t.DeletedAt == null);

            if (existingTemplate != null)
            {
                throw new InvalidOperationException($"A template with the name '{command.Request.Name}' already exists for this user.");
            }

            // Get user role
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == command.CreatedByUserId);

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {command.CreatedByUserId} not found.");
            }

            var ownerRole = user.Role?.RoleName ?? "SalesRep";

            // Create template entity
            var template = new QuotationTemplate
            {
                TemplateId = Guid.NewGuid(),
                Name = command.Request.Name,
                Description = command.Request.Description,
                OwnerUserId = command.CreatedByUserId,
                OwnerRole = ownerRole,
                Visibility = visibility,
                IsApproved = false,
                Version = 1,
                UsageCount = 0,
                DiscountDefault = command.Request.DiscountDefault,
                Notes = command.Request.Notes,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            // Create line items
            var lineItems = new List<QuotationTemplateLineItem>();
            for (int i = 0; i < command.Request.LineItems.Count; i++)
            {
                var lineItemRequest = command.Request.LineItems[i];
                var lineItem = _mapper.Map<QuotationTemplateLineItem>(lineItemRequest);
                lineItem.LineItemId = Guid.NewGuid();
                lineItem.TemplateId = template.TemplateId;
                lineItem.SequenceNumber = i + 1;
                lineItem.CalculateAmount();
                lineItem.CreatedAt = DateTimeOffset.UtcNow;
                lineItems.Add(lineItem);
            }

            template.LineItems = lineItems;

            // Save to database
            _db.QuotationTemplates.Add(template);
            await _db.SaveChangesAsync();

            // Load with navigation properties for mapping
            var savedTemplate = await _db.QuotationTemplates
                .Include(t => t.OwnerUser)
                .Include(t => t.LineItems)
                .FirstOrDefaultAsync(t => t.TemplateId == template.TemplateId);

            // Map to DTO
            var result = _mapper.Map<QuotationTemplateDto>(savedTemplate);

            _logger.LogInformation("Template {TemplateId} created successfully", result.TemplateId);

            return result;
        }
    }
}

