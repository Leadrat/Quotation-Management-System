using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.TaxManagement.Commands;
using CRM.Application.TaxManagement.Dtos;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.TaxManagement.Commands.Handlers
{
    public class UpdateTaxRateCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public UpdateTaxRateCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<TaxRateDto> Handle(UpdateTaxRateCommand cmd)
        {
            var entity = await _db.TaxRates
                .FirstOrDefaultAsync(tr => tr.TaxRateId == cmd.TaxRateId);

            if (entity == null)
            {
                throw new InvalidOperationException($"Tax rate with ID '{cmd.TaxRateId}' not found");
            }

            // Validate tax framework exists
            var framework = await _db.TaxFrameworks
                .FirstOrDefaultAsync(f => f.TaxFrameworkId == cmd.TaxFrameworkId && f.DeletedAt == null);
            if (framework == null)
            {
                throw new InvalidOperationException($"Tax framework with ID '{cmd.TaxFrameworkId}' not found");
            }

            // Validate jurisdiction if provided
            Jurisdiction? validatedJurisdiction = null;
            if (cmd.JurisdictionId.HasValue)
            {
                validatedJurisdiction = await _db.Jurisdictions
                    .FirstOrDefaultAsync(j => j.JurisdictionId == cmd.JurisdictionId.Value && j.DeletedAt == null);
                if (validatedJurisdiction == null)
                {
                    throw new InvalidOperationException($"Jurisdiction with ID '{cmd.JurisdictionId.Value}' not found");
                }
            }

            // Validate category if provided
            if (cmd.ProductServiceCategoryId.HasValue)
            {
                var category = await _db.ProductServiceCategories
                    .FirstOrDefaultAsync(c => c.CategoryId == cmd.ProductServiceCategoryId.Value && c.DeletedAt == null);
                if (category == null)
                {
                    throw new InvalidOperationException($"Category with ID '{cmd.ProductServiceCategoryId.Value}' not found");
                }
            }

            // Validate effective date overlap (excluding current rate)
            var overlappingRates = await _db.TaxRates
                .Where(tr =>
                    tr.TaxRateId != cmd.TaxRateId &&
                    tr.JurisdictionId == cmd.JurisdictionId &&
                    tr.ProductServiceCategoryId == cmd.ProductServiceCategoryId &&
                    tr.TaxFrameworkId == cmd.TaxFrameworkId &&
                    ((cmd.EffectiveFrom >= tr.EffectiveFrom && (tr.EffectiveTo == null || cmd.EffectiveFrom <= tr.EffectiveTo.Value)) ||
                     (cmd.EffectiveTo.HasValue && cmd.EffectiveTo.Value >= tr.EffectiveFrom && (tr.EffectiveTo == null || cmd.EffectiveTo <= tr.EffectiveTo.Value)) ||
                     (cmd.EffectiveFrom <= tr.EffectiveFrom && (!cmd.EffectiveTo.HasValue || cmd.EffectiveTo >= tr.EffectiveFrom))))
                .ToListAsync();

            if (overlappingRates.Any())
            {
                throw new InvalidOperationException("Tax rate with overlapping effective dates already exists for this jurisdiction and category");
            }

            entity.JurisdictionId = cmd.JurisdictionId;
            entity.TaxFrameworkId = cmd.TaxFrameworkId;
            entity.ProductServiceCategoryId = cmd.ProductServiceCategoryId;
            entity.Rate = cmd.TaxRate;
            entity.EffectiveFrom = cmd.EffectiveFrom;
            entity.EffectiveTo = cmd.EffectiveTo;
            entity.IsExempt = cmd.IsExempt;
            entity.IsZeroRated = cmd.IsZeroRated;
            entity.Description = string.IsNullOrWhiteSpace(cmd.Description) ? null : cmd.Description.Trim();
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            // Set tax components
            var componentRates = cmd.TaxComponents.Select(c => new TaxComponentRate
            {
                Component = c.Component,
                Rate = c.Rate
            }).ToList();
            entity.SetTaxComponentRates(componentRates);

            await _db.SaveChangesAsync();

            // Log configuration change
            var jurisdiction = await _db.Jurisdictions
                .Include(j => j.Country)
                .FirstOrDefaultAsync(j => j.JurisdictionId == entity.JurisdictionId);
            
            var taxLog = new TaxCalculationLog
            {
                LogId = Guid.NewGuid(),
                QuotationId = null,
                ActionType = TaxCalculationActionType.ConfigurationChange,
                CountryId = jurisdiction?.CountryId,
                JurisdictionId = entity.JurisdictionId,
                CalculationDetails = JsonSerializer.Serialize(new
                {
                    Action = "TaxRateUpdated",
                    TaxRateId = entity.TaxRateId,
                    TaxRate = entity.Rate,
                    EffectiveFrom = entity.EffectiveFrom,
                    EffectiveTo = entity.EffectiveTo,
                    IsExempt = entity.IsExempt,
                    IsZeroRated = entity.IsZeroRated,
                    TaxComponents = entity.GetTaxComponentRates()
                }),
                ChangedByUserId = cmd.UpdatedByUserId,
                ChangedAt = DateTimeOffset.UtcNow
            };
            _db.TaxCalculationLogs.Add(taxLog);
            await _db.SaveChangesAsync();

            // Reload with related data
            entity = await _db.TaxRates
                .Include(tr => tr.Jurisdiction)
                .Include(tr => tr.ProductServiceCategory)
                .Include(tr => tr.TaxFramework)
                .FirstOrDefaultAsync(tr => tr.TaxRateId == cmd.TaxRateId);

            var dto = _mapper.Map<TaxRateDto>(entity);
            if (entity?.Jurisdiction != null)
            {
                dto.JurisdictionName = entity.Jurisdiction.JurisdictionName;
            }
            if (entity?.ProductServiceCategory != null)
            {
                dto.CategoryName = entity.ProductServiceCategory.CategoryName;
            }
            if (entity?.TaxFramework != null)
            {
                dto.FrameworkName = entity.TaxFramework.FrameworkName;
            }

            return dto;
        }
    }
}

