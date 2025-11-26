using System;
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
    public class UpdateCountryCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public UpdateCountryCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<CountryDto> Handle(UpdateCountryCommand cmd)
        {
            var entity = await _db.Countries
                .FirstOrDefaultAsync(c => c.CountryId == cmd.CountryId && c.DeletedAt == null);
            
            if (entity == null)
            {
                throw new InvalidOperationException($"Country with ID '{cmd.CountryId}' not found");
            }

            // Validate country code uniqueness if changed
            if (!string.IsNullOrWhiteSpace(cmd.CountryCode) && cmd.CountryCode != entity.CountryCode)
            {
                var codeUpper = cmd.CountryCode.Trim().ToUpperInvariant();
                var exists = await _db.Countries.AnyAsync(c => 
                    c.CountryId != cmd.CountryId && 
                    c.CountryCode.ToUpper() == codeUpper && 
                    c.DeletedAt == null);
                if (exists)
                {
                    throw new InvalidOperationException($"Country with code '{cmd.CountryCode}' already exists");
                }
                entity.CountryCode = codeUpper;
            }

            // Validate country name uniqueness if changed
            if (!string.IsNullOrWhiteSpace(cmd.CountryName) && cmd.CountryName != entity.CountryName)
            {
                var nameLower = cmd.CountryName.Trim().ToLowerInvariant();
                var exists = await _db.Countries.AnyAsync(c => 
                    c.CountryId != cmd.CountryId && 
                    c.CountryName.ToLower() == nameLower && 
                    c.DeletedAt == null);
                if (exists)
                {
                    throw new InvalidOperationException($"Country with name '{cmd.CountryName}' already exists");
                }
                entity.CountryName = cmd.CountryName.Trim();
            }

            // If setting as default, unset other defaults
            if (cmd.IsDefault.HasValue && cmd.IsDefault.Value && !entity.IsDefault)
            {
                var existingDefaults = await _db.Countries
                    .Where(c => c.CountryId != cmd.CountryId && c.IsDefault && c.DeletedAt == null)
                    .ToListAsync();
                foreach (var existing in existingDefaults)
                {
                    existing.IsDefault = false;
                    existing.UpdatedAt = DateTimeOffset.UtcNow;
                }
                entity.IsDefault = true;
            }
            else if (cmd.IsDefault.HasValue && !cmd.IsDefault.Value)
            {
                entity.IsDefault = false;
            }

            if (cmd.TaxFrameworkType.HasValue)
                entity.TaxFrameworkType = cmd.TaxFrameworkType.Value;

            if (!string.IsNullOrWhiteSpace(cmd.DefaultCurrency))
                entity.DefaultCurrency = cmd.DefaultCurrency.Trim().ToUpperInvariant();

            if (cmd.IsActive.HasValue)
                entity.IsActive = cmd.IsActive.Value;

            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            // Log configuration change
            var taxLog = new TaxCalculationLog
            {
                LogId = Guid.NewGuid(),
                QuotationId = null,
                ActionType = TaxCalculationActionType.ConfigurationChange,
                CountryId = entity.CountryId,
                JurisdictionId = null,
                CalculationDetails = JsonSerializer.Serialize(new
                {
                    Action = "CountryUpdated",
                    CountryId = entity.CountryId,
                    CountryName = entity.CountryName,
                    CountryCode = entity.CountryCode,
                    TaxFrameworkType = entity.TaxFrameworkType,
                    IsDefault = entity.IsDefault,
                    IsActive = entity.IsActive
                }),
                ChangedByUserId = cmd.UpdatedByUserId,
                ChangedAt = DateTimeOffset.UtcNow
            };
            _db.TaxCalculationLogs.Add(taxLog);
            await _db.SaveChangesAsync();

            return _mapper.Map<CountryDto>(entity);
        }
    }
}

